#!/bin/bash
# 세션 시작/재개(startup, resume, clear, compact) 시 origin/master의 최신 커밋을
# 자동으로 받아오고, 커밋 작성자를 강제하는 pre-commit 훅이 항상 적용되게 한다.
# (Cluade_For_spt 저장소의 동일 패턴 — 이 레포는 기본 브랜치가 master다.)
set -uo pipefail

cd "${CLAUDE_PROJECT_DIR:-$(dirname "$0")/../..}" || exit 0

git rev-parse --is-inside-work-tree >/dev/null 2>&1 || exit 0

git config core.hooksPath .githooks 2>/dev/null || true

git fetch origin master --quiet 2>/dev/null || exit 0

LOCAL=$(git rev-parse HEAD 2>/dev/null || echo "")
REMOTE=$(git rev-parse origin/master 2>/dev/null || echo "")

if [ -n "$LOCAL" ] && [ -n "$REMOTE" ] && [ "$LOCAL" != "$REMOTE" ]; then
  if git diff --quiet 2>/dev/null && git diff --cached --quiet 2>/dev/null; then
    if git merge --ff-only origin/master --quiet 2>/dev/null; then
      echo "HealingAutoCancel-zzap--Bootleg-: origin/master 최신 내용으로 갱신했습니다 ($LOCAL -> $REMOTE)."
    fi
  fi
fi

exit 0
