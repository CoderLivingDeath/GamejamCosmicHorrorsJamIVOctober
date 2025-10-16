## MonoCharacterController

### Свойства

- **MovementCoroutine** — корутина движения персонажа.
- **DirectionMonitoringCoroutine** — корутина контроля направления движения.
- **IsMovingMonitoringCoroutine** — корутина отслеживания движения.
- **InteractionMonitoringCoroutine** — корутина мониторинга взаимодействия.
- **IsAnimating** — флаг проигрывания анимации.
- **MovementStateMachine** — машина состояний движения.
- **MovementState** — текущее состояние движения.
- **VelocityVector** — текущий вектор скорости.
- **MaxVelocity** — максимальная скорость (чтение/запись).
- **IsMoving** — движется ли персонаж.
- **CanMove** — возможность перемещения.
- **CharacterController** — компонент Unity CharacterController.
- **LastMovementVector** — последний вектор движения.
- **Animator** — компонент Animator.


### Методы

- **MoveToDirection(Vector2 direction)** — переместить персонажа в направлении.
- **MoveFromOffset(Vector3 offset)** — сдвинуть позицию относительно текущей.
- **Teleport(Vector3 point)** — мгновенная телепортация.
- **Interact()** — взаимодействие с ближайшим объектом.
- **InteractWith(InteractableBehaviour interactable)** — взаимодействие с конкретным объектом.
- **FireAnimationTrigger(string key)** — установить триггер анимации.
- **PlayAnimation(string key/IScriptableAnimation animation)** — проиграть анимацию по ключу или объекту.


### Поля (сериализуемые через инспектор)

- `inspectorMaxVelocity` — макс. скорость в инспекторе.
- `inspectorRadius` — радиус взаимодействия.
- `inspectorMask` — маска слоев взаимодействия.
- `animator` — ссылка на Animator.
- `unityCharacterController` — ссылочный CharacterController.
- `animationLibrary` — словарь анимаций.

***

## AnimationController

### Свойства

- **IsAnimating** — идет ли проигрывание анимации.
- **Animator** — Unity Animator.


### Методы

- **PlayAnimation(string key, bool override)** — проиграть анимацию по ключу.
- **PlayAnimation(IScriptableAnimation animation, bool override)** — проиграть анимацию объектом.
- **FireAnimationTrigger(string trigger)** — установить триггер Animator.
- **CancelCurrentAnimation()** — отменить текущую анимацию.
- **ValidateAnimator(Animator animator)** — проверить корректность настроек Animator.

***

## MovementController

### Свойства

- **MovementStateMachine** — машина состояний движения.
- **State** — текущее состояние.
- **MovementCoroutine**, **DirectionMonitoringCoroutine**, **IsMovingMonitoringCoroutine** — корутины движения и мониторинга.
- **Velocity** — текущая скорость.
- **VelocityVector** — вектор текущей скорости.
- **MaxVelocity** — максимальная скорость (чтение/запись).
- **IsMoving** — движется ли персонаж.
- **CanMove** — доступность движения.
- **CharacterController** — Unity CharacterController.


### Методы

- **MoveToDirection(Vector2 direction)** — задать направление движения.
- **MoveTo(Vector3 point, float distance)** — движение к точке.
- **MoveFromOffset(Vector3 offset)** — движение относительно позиции.
- **Teleport(Vector3 position)** — мгновенная телепортация.
- **Enable()** — включить корутины движения.
- **Disable()** — отключить корутины.
- **Dispose()** — освобождение ресурсов и остановка корутин.


### События

- **OnStateChanged** — событие смены состояния движения.
- **OnPositionChanged** — событие изменения позиции.

***

## InteractionController

### Свойства

- **Transform** — трансформ персонажа.
- **Radius** — радиус поиска интерактивных объектов.
- **Mask** — маска слоев для поиска.
- **Timing** — время работы корутины.
- **MonitoringCoroutine** — корутина мониторинга.
- **SelectedInteractable** — ближайший интерактивный объект.
- **Interactables** — все интерактивные объекты в радиусе.


### Методы

- **FindInteractables(Vector3 origin, float radius, LayerMask mask)** — поиск интерактивных объектов.
- **GetInteractables(Vector3 position)** — получить интерактивные объекты с сортировкой.
- **Interact()** — взаимодействовать с ближайшим объектом.
- **InteractWith(InteractableBehaviour interactableBehaviour)** — взаимодействовать с указанным объектом.
- **Enable()** — запуск мониторинга.
- **Disable()** — остановка мониторинга.
- **Dispose()** — освобождение ресурсов, остановка корутины.