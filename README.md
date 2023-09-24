<!-- 
<img src="" width="640" height="360">
-->

# Unity2DShootingGame
유니티와 포톤, 파이어베이스를 이용한 실시간 2인 대전 게임 개인 프로젝트입니다.

## 개발 환경
- Unity(2021.3.2f1)
- Photon Network
- Firebase

## 로그인
Firebase Auth를 이용해 계정을 등록하고 로그인을 할 수 있음

Firebase Firestore도 연동하기에 계정 정보, 닉네임 정보 등 중복성을 확인한 후 가입 및 로그인 기능을 구현함

**[구동 화면]**

- 회원 가입

<img src="https://github.com/SeungJun-Kwon/Unity2DShootingGame/assets/80217301/1693c784-80ad-461c-a695-c2a2d70e1291" width="640" height="360">

- 로그인

<img src="https://github.com/SeungJun-Kwon/Unity2DShootingGame/assets/80217301/de7ee3b3-6070-4573-af56-cd56fe7770a4" width="640" height="360">

## 로비
로그인 이후 나타나는 로비 화면

포톤의 콜백 함수인 ```OnRoomListUpdate```을 활용하여 방이 업데이트될 때마다 로비에서 보여지는 방 리스트도 같이 업데이트 되도록 구현함

각 방 마다 방의 이름, 맵 정보, 비밀번호가 존재하기 때문에 포톤 Room의 CustomProperties를 사용함

방의 정보, 비밀번호를 로비에서도 확인할 수 있도록 하기 위해 ```CustomRoomPropertiesForLobby``` 함수를 사용함

**[구동 화면]**

<img src="https://github.com/SeungJun-Kwon/Unity2DShootingGame/assets/80217301/e03421ac-2acc-49b8-ba68-0243923d38c8" width="640" height="360">

## 유저 정보
로비 및 룸 화면에서 확인할 수 있는 유저 정보

회원 가입 시 자동으로 파이어베이스 파이어스토어에 유저 정보가 등록된다.

또한 UserInfo 클래스와 안에 있는 변수들을 파이어스토어에 매핑하기 위해 FirestoreData, FirestoreProperty를 사용함

<img src="https://github.com/SeungJun-Kwon/Unity2DShootingGame/assets/80217301/bb2632e1-cb59-4e6b-ae03-8f742362446d" width="640" height="360">

파이어베이스와 연동하여 게임 내에서 유저 정보를 Save, Load가 가능함

**[구동 화면]**

<img src="https://github.com/SeungJun-Kwon/Unity2DShootingGame/assets/80217301/706490ba-5549-4c8b-9e5a-895caba380c2" width="640" height="360">

## 방 생성, 입장
로비에서 룸을 선택해 입장을 누르면 표시되는 화면

포톤 네트워크를 통해 룸 목록을 로비에서 확인할 수 있고 입장하면 룸에 있는 유저와 통신할 수 있음

상대방 프로필을 누르면 간단한 정보를 볼 수 있고 채팅 또한 가능함

룸 안에서도 프로필을 변경할 수 있으며 상대방 또한 나의 프로필을 언제든 확인할 수 있음

**[구동 화면]**

<img src="https://github.com/SeungJun-Kwon/Unity2DShootingGame/assets/80217301/396c4260-3d7f-4d11-8ed2-217f79edb0be" width="640" height="360">

## 상태 패턴
IState라는 인터페이스를 만들어 각 상태 트리거를 구현하고 동작할 수 있도록 한 패턴

<img src="https://github.com/SeungJun-Kwon/Unity2DShootingGame/assets/80217301/39324076-8a96-4fb9-85e3-ce7a2c36588d" width="500" height="400">


```Dictionary<State, IState> _stateDic = new Dictionary<State, IState>()```

각 PlayerController 객체는 상태를 필요할 때 꺼내 쓸 수 있도록 딕셔너리에 캐싱했고 자신이 가진 상태들을 확인하고 관리하기 위해 List에 추가, 제거할 수 있도록 했음

특수한 상황을 제외하고는 상태에 중복 진입하면 안되기 때문에 상태가 있는지 먼저 찾는 ```FindState``` 함수를 통해 해당 상태가 없다면 진입하여

해당 상태 인터페이스 클래스에서 OnEnter를 통해 상태 진입 트리거가 발생하도록 했음

IDLE이나 MOVE 상태처럼 상시적으로 동작을 행해야 하는 상태의 경우 ```OnUpdate```, ```OnFixedUpdate``` 트리거를 구현해

각 PlayerController 객체의 ```Update```, ```FixedUpdate``` 메소드에서 호출하도록 했음

상태에서 빠져나갈 때 또한 ```OnExit``` 트리거를 실행함

각 상태를 단일로 관리해 제어하는 것이 아니라 List로 관리하는 이유는 이동하는 중에 점프, 점프 중에 공격 등과 같은 동작들을 위해서인데

상태를 단일 상태로 제어할 경우 이동 중에 점프가 아니라 MOVE -> JUMP -> IDLE or MOVE 형태로 중간 중간 동작이 끊겨 부자연스러워지기 때문임

**[구동 화면]**

<img src="https://github.com/SeungJun-Kwon/Unity2DShootingGame/assets/80217301/7c2a99e1-da29-4bcc-b9ff-b5d21d4971d3" width="640" height="360">

## 무기, 업그레이드 선택
게임 진행 중 무기, 업그레이드 선택지가 주어짐

