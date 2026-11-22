# Sdx17 NPC Framework — Selling Requirements

> **Do not submit to the Asset Store until every box below is
> checked.** Version 0.1.0 is currently **UNTESTED IN UNITY**.
> This checklist is the gate between "docs written" and "ready to
> sell". If any item fails, fix it and re-check — do not submit
> around a failing item.

## 1. Unity verification (the hard gate)

- [ ] **Clean import, zero errors/warnings.** Import the package
  into a clean Unity **2022.3 LTS** project (new project, no other
  packages). The Console shows **zero errors and zero warnings**
  on import. Any warning must be fixed or explicitly documented
  as benign — "one small warning, probably fine" is not a pass.
- [ ] **Demo scene plays.** Build the demo scene by hand from
  `Samples~/Demo/README.md` (plane, baked NavMesh, Player-tagged
  capsule, NPC, `NPCDemo` driver). Press Play: the NPC patrols,
  chases and attacks in range, flees below its flee-health
  fraction, dies at 0 health, and the spawner respawns. Pressing
  **I** logs the interaction hook and **K** deals 25 damage.
- [ ] **Self-test passes.** Add `NPCSelfTest` to a GameObject,
  press Play, and confirm the summary line reads **0 FAIL**.
- [ ] **TEST checklist complete.** Every item in the TEST checklist
  in `README.md` has been run in Unity 2022.3 LTS and checked off
  (definition inspector, menu items, patrol, aggro, attack, flee,
  death, interaction, spawner). Update `README.md`, `GUIDE.md`,
  and `LISTING.md` to remove the "UNTESTED IN UNITY" warnings
  only after this is done — and only for the behaviour actually
  verified.

## 2. Documentation proofread

- [ ] **README + Guide proofread.** `README.md` and
  `Documentation~/GUIDE.md` read end to end: no stand-in text,
  no invented class or method names, every API entry matches the
  code in `Runtime/`, every cross-reference (file paths, menu
  paths, key bindings) is correct. Fix the guide if the code
  changed since it was written — if a section disagrees with the
  code, the code is right.
- [ ] **LISTING.md proofread.** Title, short description (under
  200 characters — re-count after any edit), long description,
  keywords, price, and version history all match the package as
  it actually is. No claim of testing, sales, or reviews unless
  it is true at submission time.
- [ ] **ThirdPartyNotices.txt accurate.** Still true that no
  third-party code, fonts, audio, or assets are included. If
  anything third-party was added since, list it with its licence
  before submitting.

## 3. Screenshots — real Unity screenshots only

- [ ] **At least 5 screenshots**, captured from the real demo
  scene running in Unity. **1920x1080** each.
- [ ] **Real Unity screenshots only.** Screenshots must show the
  actual demo scene (NPC patrolling, chasing, attacking, the
  Inspector with a definition, the spawner in action — pick 5+
  that show different things). **No AI images presented as
  screenshots.** Concept art is not a screenshot and must never
  be uploaded as one.
- [ ] Screenshots are clear, well-lit, and readable at listing
  size. No Console errors visible in any shot. No stand-in or
  default-scene look that misrepresents the package.

## 4. Video — real Unity recording only

- [ ] **30–60 second video**, a real screen recording of the demo
  scene in Unity (Play mode): NPC patrolling, player approaching,
  chase and attack, an interaction, damage / flee / death if it
  fits in the time. No cuts that hide behaviour.
- [ ] **Real Unity recording only.** No AI-generated video, no
  slideshow of still images, no footage of a different project
  presented as this package.
- [ ] Video has no copyrighted music. Narration or on-screen text
  is optional; if used, it must describe only what the package
  actually does.

## 5. Listing assets

- [ ] **Icon 512x512.** A clean, readable icon for the package
  (the Sdx17 mark or a simple NPC glyph). Readable at small size;
  no text so small it becomes noise.
- [ ] **AI-content disclosure.** If any concept art is used
  anywhere in the listing (cover image, extra images), disclose
  it as AI-generated where the Asset Store asks, and never present
  it as a screenshot or as in-engine footage. If no AI art is
  used, nothing to disclose — do not claim AI art was used.
- [ ] **Category re-checked.** Confirm the category at listing
  time (suggested: Tools / Game Toolkits — see `LISTING.md`).
  The Asset Store category list changes; pick the closest real
  fit in the publisher portal.

## 6. Publisher account and money

- [ ] **Publisher account set up**, with **2FA enabled**.
- [ ] **Tax information submitted** in the publisher portal
  (required before payout).
- [ ] **PayPal set up** for payouts, and verified working.
- [ ] **Price re-checked.** Re-check comparable NPC/AI toolkit
  prices and the $15–$35 first-review band before listing (see
  `LISTING.md` — suggested price $25, 70% net = $17.50 per sale
  before tax/refunds). If the market has moved, adjust the price
  and update `LISTING.md`. Price does not guarantee sales.

## 7. Submission rules (know them before you submit)

- [ ] **Review takes at least 5 business days.** Plan for it;
  do not promise buyers or yourself a launch date inside that
  window. A rejection restarts the clock — fix and resubmit.
- [ ] **5 submissions/month cap in the first 6 months.** New
  publishers are limited to 5 submissions per month for the
  first 6 months (per Unity's publisher rules at time of
  writing — re-check in the publisher portal). Do not burn
  submissions on unready packages; each rejection costs one.
- [ ] **One package, one submission.** Submit the NPC Framework
  on its own. Do not bundle other Sdx17 systems into this
  submission unless they are verified, documented, and listed as
  included.

## 8. Final honesty check (read aloud before submitting)

- [ ] The listing claims **no sales, no reviews, and no testing
  that did not happen**. Version 0.1.0's test status is stated
  truthfully — if it was tested in Unity by submission time, say
  so and say what was tested; if not, it is not submitted.
- [ ] No fake email, no fake testimonial, no fake screenshot, no
  AI image presented as a screenshot, no price that guarantees
  sales.
- [ ] The user (Sdx17) has read the final listing text and
  approved it before submission.

---

*Checklist for v0.1.0, written 2026-11-21. Update the Unity
version, review time, and submission cap figures at submission
time if Unity's rules have changed.*
