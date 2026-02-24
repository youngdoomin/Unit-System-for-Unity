# Unit System for Unity

유지보수와 확장성을 고려한 유닛 시스템입니다
인터페이스, 추상클래스, ScriptableObject를 활용하여 다양한 유닛을 추가 및 수정이 용이하도록 설계됐습니다

## 주요기능

* **Class-based FSM**: 상태를 클래스 단위로 캡슐화하여 Monobehaviour의 의존성을 줄이고, 상태 추가 및 수정이 용이합니다
* **Decoupled Architecture**: 인터페이스와 상속 구조를 활용하여 기능사이의 결합도를 낮췄습니다
* **Data-Driven Design**: `ScriptableObject`를 통해 State, 상태이상을 관리하여 수정이 용이합니다

## Architecture

### 1. 상태패턴 (FSM)

각 유닛들은 BaseFsm으로 상태 제어를 하며, 모든 FSM의 상태는 클래스 `BaseState`를 상속받아 구현됩니다

* `Enter()`: 상태 진입 시 초기화
* `Update()`: 매 프레임 실행될 로직 (AI 판단 등)
* `Exit()`: 상태 전환 시 정리 로직

#### 1.1 State 확장 용이성
같은 State를 사용하더라도 State에 맞는 프리셋을 갈아끼울 수 있게 설계를 하였습니다. 그렇기에 확장성이 뛰어나며 테스트 및 협업에 유용합니다

**사용법**

1. State 타입에 대응하는 SO를 에디터에서 생성합니다
2. 생성한 SO를 Fsm에 적용합니다
(자세한 예시는 CommonState 폴더를 확인해주세요)

### 2. 상호작용 시스템
게임내 상호작용 요소를 모듈화하여 수정 및 확장에 유용합니다.

#### 2.1 부위 데미지 시스템
데미지 처리를 DamageablePart에서 하며 이 부위에 맞을시 체력이 얼마나 감소할지, 부위별 피격효과와 소리가 어떻게 될지 등이 별개로 설정 가능합니다.

**사용법**

Health에 DamageablePart를 연결하면 서로 연동됩니다.

#### 2.2 상태이상 시스템
상태이상 시스템을 별개의 상태이상이 별개로 실행되며 유닛별로 관리가능하게 설계되었습니다.
같은 상태이상도 1.1 State 확장 용이성처럼 SO로 프리셋을 다르게 설정가능합니다. 

전체 코드는 StatusEffect 폴더에서 확인 가능합니다.

### 3. 수정이 용이한 애니메이션 시스템

유니티의 애니메이션 시스템은 구조적 수정이 없다면 애니메이터의 변수를 만들고 코드에서 제어하여 그 결과값에 따라 애니메이터가 어느 애니메이션에 전이할지 정하는 불편한 구조를 가지고 있습니다.
해당 아키텍처는 어느 애니메이션을 실행할지 코드 내에서 직접 선택 가능하게 합니다.

**사용법**

애니메이터는 변수나 Transition을 만들 필요 없이 노드를 생성하여 코드에서 실행할 이름을 정하고 대응하는 애니메이션을 넣어주기만하면 됩니다.
코드는 BaseFSM의 원하는 State에서 BaseAnimationController을 Play 또는 CrossFade를 하면 됩니다.

전체 코드는 Animation 폴더에서 확인 가능합니다

### 폴더 구조
```
Folder Structure
Unit-System-for-Unity/
├── Animation/          # Animation controller system
├── CommonState/        # Shared FSM state examples
├── EnemyModule/        # Enemy-specific unit modules
├── Fsm/                # Base FSM and state classes
└── StatusEffect/       # Status effect system
```
