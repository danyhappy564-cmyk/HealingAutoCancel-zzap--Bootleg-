# HealingAutoCancel (SPT 4.1 포팅) — zzap--Bootleg-

---

### ⚠️ IMPORTANT NOTICE / DISCLAIMER

**Original Author:** IhanaMies (Mikael Palokangas)
**Original Repository:** HealingAutoCancel
**Original Link:** https://github.com/IhanaMies/HealingAutoCancel
**License notation:** MIT License (원본 `LICENSE.txt` 그대로 유지)

1. **Reflection & Take-Downs:** 저는 ECOT 사건을 깊이 반성하며, AI 도움을 받는 "바이브 코더"로서 원작자가 요청하면 즉시 파일을 삭제합니다.
2. **No Re-Distribution:** 이 포팅 빌드는 검증되지 않은 임시 수정본입니다. 다른 곳에 재업로드·공유하지 말아 주세요.
3. **Do Not Pester Original Authors:** 이 비공식 포팅에서 발생한 문제로 원작자를 귀찮게 하거나 버그를 제보하지 마세요.
4. **Full Credit & Respect:** 항상 원작자를 GitHub에서 크레딧하며, 원작자의 결정을 최우선으로 존중합니다.
5. **Support Original Creators:** 이 포팅 대신, 원작자의 Forge 페이지를 방문해서 응원의 말을 남기거나 후원해 주세요.

---

## 이 모드가 하는 일

붕대/의료품(메드킷, 지혈대 등)으로 치료하는 도중, **해당 부위가 이미 최대 체력이고
출혈도 없거나(더 치료할 게 없음), 아이템의 사용 가능 자원(HP)이 다 떨어지면**
치료 애니메이션을 자동으로 취소합니다. 치료가 끝날 때까지 굳이 기다리지 않아도 됩니다.

- 설정(BepInEx F12 설정 창): `Heal` 카테고리 → `Enable automatic heal canceling` (기본값 On)

## 원작 대비 변경 사항 (SPT 4.0 → 4.1 포팅)

`Cluade_For_spt` 지식 베이스(`docs/SPT-4.1-PORTING-KB.md`)의 클라 포팅 절차를
따라 진행했고, 이번 포팅에서 실제로 걸린 함정은 다음과 같습니다 (해당 KB에도
새로 반영했습니다):

| 4.0 | 4.1 | 확인 방법 |
|---|---|---|
| `using SPT.Reflection.Patching;` / `class X : ModulePatch` | `using SPTarkov.Reflection.Patching;` / `class X : AbstractPatch` | `SPTarkov.Reflection.dll`(버전 `4.1.5.0`)의 `TypeDef` 전수 조회 — `ModulePatch`는 없고 `AbstractPatch`만 존재, `PatchPrefixAttribute`/`PatchPostfixAttribute`는 이름 그대로 유지 |
| `MedsItemClass` | `EFT.InventoryLogic.Meds` | 실제 `Assembly-CSharp.dll`에서 `MedsItemClass` TypeDef 0건, `EFT.InventoryLogic.Meds`가 `MedKitComponent` 필드를 그대로 들고 있음을 확인 |
| `DamageInfoStruct` | `EFT.Ballistics.DamageInfo` | `ActiveHealthController.HealthChangedEvent`의 델리게이트 시그니처(IL `TypeSpec` 직접 디코딩)가 `Action<EBodyPart, float, EFT.Ballistics.DamageInfo>`임을 확인 |
| `TargetFramework net472` + 하드코딩된 `D:\SPT Iterations\...` HintPath | `TargetFramework netstandard2.1` + `SptRoot`(기본값 `E:\SPT 4.1`) 기반 `$(Managed)`/`$(SptRuntime)` 프로퍼티 | KB 5절 클라 플러그인 표준 템플릿 |
| `SPTarkov.Reflection.dll` 위치를 `BepInEx\plugins\spt`로 추정 | 실제 설치본 확인 결과 `E:\SPT 4.1\SPT_Runtime\SPTarkov.Reflection.dll` — `SptRuntime` 프로퍼티로 분리 | 사용자 확인 (2026-09-18) |
| 진짜 SPT 설치본 검증 없음 | `EnsureRealSptReflection` 빌드 가드 추가 — 빌드에 쓰는 `SPTarkov.Reflection.dll`이 `1.0.0.0`(플레이스홀더)이면 빌드 자체를 실패시킴 | KB 2.1절 (런처가 참조 dll 버전으로 "빌드된 SPT 버전"을 판정하는 것에 대한 대응) |

그 외 로직(부위 체력·출혈 판정, `RemoveMedEffect()` 호출, 이벤트 구독 방식)은
`GetBodyPartHealth` / `IsBodyPartBroken` / `BodyPartEffects` 모두 4.1의
`EFT.HealthSystem.BaseHealthController<T>`(제네릭 베이스 클래스)에 이름 그대로
살아있음을 확인해서 **손대지 않았습니다.**

## 컨테이너에서 검증한 것 / 못 한 것

- **검증함:** 위 표의 모든 타입/멤버 이름을 `Cluade_For_spt/references/Assembly-CSharp.dll`,
  `SPTarkov.Reflection.dll`(SPT 4.1 실제 게임/플러그인 어셈블리)에 대해 `dnfile`로
  IL 메타데이터를 직접 읽어 대조했습니다. 컴파일러 없이도 "이 심볼이 실제로 존재하는가"는
  확정된 사실입니다.
- **검증 못 함(이 원격 컨테이너에는 .NET SDK와 실제 SPT 설치본이 없음):** 실제 `dotnet build`
  컴파일, BepInEx 로드, 인게임 동작 확인. **로컬 Windows(`E:\SPT 4.1`)에서 직접 빌드·설치 후
  확인이 필요합니다.**

## 빌드 방법 (로컬, Windows)

1. `E:\SPT 4.1`에 SPT 4.1.5가 설치되어 있어야 합니다(csproj의 `SptRoot` 기본값).
   다른 경로라면 환경변수 `SPT_ROOT`를 그 경로로 설정하세요.
2. Visual Studio 2022(또는 `dotnet build`)로 `ImprovedSelfcare.sln`을 빌드합니다.
3. `PostBuild` 타겟이 결과 DLL을 자동으로 `E:\SPT 4.1\BepInEx\plugins\`에 복사합니다
   (`SptRoot`를 바꿨다면 그 경로로 복사됩니다).
4. `Release` 구성으로 빌드하면 `release\HealingAutoCancel-1.0.0.zip`
   (`BepInEx/plugins/HealingAutoCancel.dll` 구조)이 자동 생성됩니다 — 배포/백업용.

## 설치 (빌드된 DLL을 받은 경우)

`HealingAutoCancel.dll`을 `E:\SPT 4.1\BepInEx\plugins\`에 넣으면 됩니다.

## 문제가 있다면

이 포팅에서 발생한 문제는 원작자가 아니라 이 포크 저장소 이슈로 남겨 주세요.
위 디스클레이머의 3번 항목대로, 원작자에게는 문의하지 말아 주세요.
