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
