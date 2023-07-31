﻿using HarmonyLib;
using Pathfinding.RVO;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

namespace ThronefallMP;

static class PlayerMovementPatch
{
	private static readonly int Moving = Animator.StringToHash("Moving");
	private static readonly int Sprinting = Animator.StringToHash("Sprinting");

	public const float MaximumDeviance = 8.0f;
	public const float MaximumDevianceSquared = MaximumDeviance * MaximumDeviance;

	public static void Apply()
	{
		On.PlayerMovement.Awake += Awake;
        On.PlayerMovement.Update += Update;
    }

	private static bool FirstInitialization = true;
	
	static void Awake(On.PlayerMovement.orig_Awake original, PlayerMovement self)
	{
		if (self.gameObject.GetComponent<PlayerNetworkData>() == null)
		{
			self.gameObject.AddComponent<PlayerNetworkData>();
		}
		
		if (PlayerMovement.instance == null)
		{
			PlayerMovement.instance = self;
		}

		if (FirstInitialization)
		{
			Plugin.Instance.Network.InitializeDefaultPlayer(self.gameObject);
			FirstInitialization = false;
		}
	}

    static void Update(On.PlayerMovement.orig_Update original, PlayerMovement self)
    {
	    var playerNetworkData = self.GetComponent<PlayerNetworkData>();
	    
        var input = Traverse.Create(self).Field<Rewired.Player>("input").Value;
        var hp = Traverse.Create(self).Field<Hp>("hp").Value;
        var rvoController = Traverse.Create(self).Field<RVOController>("rvoController").Value;
        var heavyArmorEquipped = Traverse.Create(self).Field<bool>("heavyArmorEquipped").Value;
        var racingHorseEquipped = Traverse.Create(self).Field<bool>("racingHorseEquipped").Value;
        
        var velocity = Traverse.Create(self).Field<Vector3>("velocity");
        var yVelocity = Traverse.Create(self).Field<float>("yVelocity");
        var viewTransform = Traverse.Create(self).Field<Transform>("viewTransform");
        var sprintingToggledOn = Traverse.Create(self).Field<bool>("sprintingToggledOn");
        var sprinting = Traverse.Create(self).Field<bool>("sprinting");
        var moving = Traverse.Create(self).Field<bool>("moving");
        var desiredMeshRotation = Traverse.Create(self).Field<Quaternion>("desiredMeshRotation");
        var controller = Traverse.Create(self).Field<CharacterController>("controller");
        
        // Normal code
		Vector2 zero = new Vector2(playerNetworkData.SharedData.MoveVertical, playerNetworkData.SharedData.MoveHorizontal);
		if (LocalGamestate.Instance.PlayerFrozen)
		{
			zero = Vector2.zero;
		}
		
		Vector3 normalized = Vector3.ProjectOnPlane(viewTransform.Value.forward, Vector3.up).normalized;
		Vector3 normalized2 = Vector3.ProjectOnPlane(viewTransform.Value.right, Vector3.up).normalized;
		velocity.Value = Vector3.zero;
		velocity.Value += normalized * zero.x;
		velocity.Value += normalized2 * zero.y;
		velocity.Value = Vector3.ClampMagnitude(velocity.Value, 1f);
		var shouldToggleSprint = playerNetworkData.SharedData.SprintToggleButton && !playerNetworkData.PlayerMovementSprintToggle;
		playerNetworkData.PlayerMovementSprintToggle = playerNetworkData.SharedData.SprintToggleButton;
		if (shouldToggleSprint)
		{
			sprintingToggledOn.Value = !sprintingToggledOn.Value;
		}
		if (sprintingToggledOn.Value && playerNetworkData.SharedData.SprintButton)
		{
			sprintingToggledOn.Value = false;
		}
		sprinting.Value = (playerNetworkData.SharedData.SprintButton || sprintingToggledOn.Value) && hp.HpPercentage >= 1f;
		velocity.Value *= (sprinting.Value ? self.sprintSpeed : self.speed);
		if (heavyArmorEquipped && DayNightCycle.Instance.CurrentTimestate == DayNightCycle.Timestate.Night)
		{
			velocity.Value *= PerkManager.instance.heavyArmor_SpeedMultiplyer;
		}
		if (racingHorseEquipped)
		{
			velocity.Value *= PerkManager.instance.racingHorse_SpeedMultiplyer;
		}
		rvoController.velocity = velocity.Value;
		moving.Value = velocity.Value.sqrMagnitude > 0.1f;
		if (moving.Value)
		{
			desiredMeshRotation.Value = Quaternion.LookRotation(velocity.Value.normalized, Vector3.up);
		}
		if (desiredMeshRotation.Value != self.meshParent.rotation)
		{
			self.meshParent.rotation = Quaternion.RotateTowards(self.meshParent.rotation, desiredMeshRotation.Value, self.maxMeshRotationSpeed * Time.deltaTime);
		}
		
		self.meshAnimator.SetBool(Moving, moving.Value);
		self.meshAnimator.SetBool(Sprinting, sprinting.Value);
		if (controller.Value.enabled)
		{
			if (controller.Value.isGrounded)
			{
				yVelocity.Value = 0f;
			}
			else
			{
				yVelocity.Value += -9.81f * Time.deltaTime;
			}
			
			velocity.Value += Vector3.up * yVelocity.Value;

			if (!playerNetworkData.IsLocal)
			{
				var deltaPosition = playerNetworkData.SharedData.Position - controller.Value.transform.position;
				if (deltaPosition.sqrMagnitude > MaximumDevianceSquared)
				{
					self.TeleportTo(playerNetworkData.SharedData.Position);
				}
				else
				{
					velocity.Value = Vector3.Lerp(deltaPosition, velocity.Value, 0.5f);
					controller.Value.Move(Vector3.Lerp(deltaPosition, velocity.Value * Time.deltaTime, 0.5f));
				}
			}
			else
			{
				controller.Value.Move(velocity.Value * Time.deltaTime);
				playerNetworkData.SharedData.Position = controller.Value.transform.position;
			}
		}
    }
}
