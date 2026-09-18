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
| `MedsItemClass` | `EFT.InventoryLogic.Meds` | 실제 `Assembly-CSharp.dll`에서 `MedsItemClass` TypeDef 0건, `EFT.InventoryLogic.Meds`가 `MedKitComponent` 필드를 그대로 들고 있음을 확인 — **실제 로컬 빌드에서 이 부분은 에러 없이 통과함** |
| `DamageInfoStruct` | `EFT.Ballistics.DamageInfo` | `ActiveHealthController.HealthChangedEvent`의 델리게이트 시그니처(IL `TypeSpec` 직접 디코딩)가 `Action<EBodyPart, float, EFT.Ballistics.DamageInfo>`임을 확인 — 마찬가지로 실제 빌드에서 통과 |
| `TargetFramework net472` + 하드코딩된 `D:\SPT Iterations\...` HintPath | `TargetFramework netstandard2.1` + `SptRoot`(기본값 `E:\SPT 4.1`) 기반 `$(Managed)`/`$(SptPlugins)` 프로퍼티 | 이미 4.1로 포팅·빌드 성공한 다른 포크(`SPTScopeTweak-4.1-zzap--Bootleg-`)와 동일 패턴 |
| `using SPT.Reflection.Patching;` / `class X : ModulePatch` | **바뀌지 않았습니다.** 4.0과 동일하게 유지 | ⚠️ 한 번 `SPTarkov.Reflection.Patching`/`AbstractPatch`로 잘못 바꿨다가(서버용 `SPTarkov.Reflection.dll`을 클라 플러그인에 잘못 참조) `CS0508`/`CS1705`/`System.Runtime` 버전 충돌로 빌드가 깨졌습니다. `SPTScopeTweak-4.1-zzap--Bootleg-`의 실제 빌드 성공 사례를 대조해서 원상복구했습니다 — **클라이언트용 Harmony 래퍼는 4.1에서도 여전히 `spt-reflection.dll` / `SPT.Reflection.Patching.ModulePatch`** 입니다. `SPTarkov.Reflection.dll`(`SPT_Runtime` 아래, net10.0)은 **서버용**이고 클라 플러그인과는 무관합니다. |
| 진짜 SPT 설치본 검증 없음 | `EnsureRealSptReflection` 빌드 가드 추가 — 빌드에 쓰는 `spt-reflection.dll`이 `1.0.0.0`(플레이스홀더)이면 빌드 자체를 실패시킴 | KB 2.1절 (런처가 참조 dll 버전으로 "빌드된 SPT 버전"을 판정하는 것에 대한 대응) |

그 외 로직(부위 체력·출혈 판정, `RemoveMedEffect()` 호출, 이벤트 구독 방식)은
`GetBodyPartHealth` / `IsBodyPartBroken` / `BodyPartEffects` 모두 4.1의
`EFT.HealthSystem.BaseHealthController<T>`(제네릭 베이스 클래스)에 이름 그대로
살아있음을 확인해서 **손대지 않았습니다.**

## 검증 상태

- **빌드 성공 확인됨 (2026-09-18, 사용자 로컬 Windows 빌드).** 위 표의 수정 전부
  실제 컴파일을 통과했습니다.
- **실수했다가 고침:** 처음엔 `Cluade_For_spt/references/SPTarkov.Reflection.dll`을
  "클라이언트용 Harmony 래퍼도 이 이름으로 바뀌었다"고 잘못 일반화해서
  `SPT.Reflection.Patching`/`ModulePatch`를 `SPTarkov.Reflection.Patching`/
  `AbstractPatch`로 바꿨었습니다. 실제로는 그 참조 dll이 **서버용**(net10.0)이었고,
  클라이언트는 4.0과 마찬가지로 `spt-reflection.dll`/`ModulePatch`를 씁니다. 이미
  성공적으로 빌드된 다른 4.1 포크를 직접 대조하고 나서야 확정했습니다.
- **아직 확인 안 됨:** 인게임 동작(실제로 붕대/메드킷 사용 중 자동 취소가 되는지).
  빌드·설치는 끝났으니 다음은 실전 확인입니다.

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
