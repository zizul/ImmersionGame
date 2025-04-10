# ImmersionGame

**Platform:** Unity 6000.0.28f1  

## Table of Contents
- [Overview](#overview)
- [Architecture](#architecture)
- [Unity Techniques](#unity-techniques)
- [Programming Practices](#programming-practices)
- [Gameplay Systems](#gameplay-systems)
- [Setup and Installation](#setup-and-installation)
- [Contributing](#contributing)
- [License](#license)

## Overview
This project is a 3D first-person game built using Unity and follows modern design patterns and programming practices to ensure scalability and maintainability. 
It features key gameplay systems such as custom player controller, weapons, health management, moving platforms, power-ups, and visual feedback mechanisms.

---

## Architecture

### Design Patterns
- **Component-Based Architecture**  
  - Separate components: `PlayerController`, `CameraController`, `Health`.
- **Observer Pattern**  
  - Events: `Health.OnHealthChanged`, `PlayerController.OnPlayerStateChanged`.
- **State Pattern**  
  - `PlayerState` structure in `PlayerController`.
- **Interfaces**  
  - `IDamageable` implemented by `Enemy` and `Player`.
- **Inheritance**  
  - Base class `PowerUp` extended by `SpeedBoost`.

---

## Unity Techniques
- **Serialized Fields**  
  - Example: `[SerializeField] private float moveSpeed` in controllers.
- **Rigidbody Physics**  
  - Example: `PlayerController.Move()` using `rb.velocity`.
- **Raycasting**  
  - Example: Ground check in `PlayerController.GroundCheck()` using `Physics.Raycast`.
- **Coroutines**  
  - Example: Temporal effects like `EnemyController.ShakeOnDamage()`.

---

## Programming Practices
- **Event-Based Communication**  
  - Example: `UIManager` subscribing to `Health.OnHealthChanged`.
- **Component Caching**  
  - Example: `rb = GetComponent<Rigidbody>();` in the `Start()` method.
- **Smooth Interpolation**  
  - Example: Camera movement using `SmoothDamp` in `CameraController`.
- **Object Pooling for Projectiles**  
  - Managed by a `ProjectilePool`.

---

## Gameplay Systems
- **Health System**  
  - Methods: `Health.TakeDamage()`, `Health.Heal()`.
- **Moving Platforms System**  
  - Controlled by the `MovingPlatformController`.
- **Power-Up System**  
  - Virtual methods: `PowerUp.Activate()`, `PowerUp.Deactivate()`.
- **Visual Feedback Mechanism**  
  - Material color change in the `EnemyController` upon receiving damage.

---

## Setup and Installation
1. Clone the repository:

   `git clone https://github.com/zizul/ImmersionGame.git`
2. Open the project in Unity (version 6000.0.28f1 or later).
3. Run the project from the Unity Editor or build it for your target platform.

---

## Contributing
Contributions are welcome! Please follow these steps:
1. Fork the repository.
2. Create a new branch for your feature or bugfix.
3. Submit a pull request with a clear description of changes.

---

## License
This project is licensed under the [MIT License](LICENSE). Feel free to use, modify, and distribute this project as per the terms of the license.
