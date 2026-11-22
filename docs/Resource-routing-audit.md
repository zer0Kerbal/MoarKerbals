# MoarKerbals resource-routing audit

Scope: every consuming ability in the source and installed ModuleManager.ConfigCache; checks, payments, refunds, accident attempts and additional births. Inventory: three consuming module types, 14 ability instances across 13 parts. Large_Crewed_Lab has two abilities. Passive crew-capacity and IVA-seat modules do not consume recipe resources.

| Module | Installed parts carrying the ability | Before 1.3.8 |
| --- | --- | --- |
| KloneBay | moar-klonebay-ground; moar-klonebay-orbital; MK1CrewCabin; mk3CrewCabin; Large_Crewed_Lab | Still used native resource routing in installed 1.3.6 and prepared 1.3.7; affected by the same class of stack-crossfeed exclusion. |
| KerbalRecruitment | moar-kloneschool; moar-kloneschool-orbital; cupola; Large_Crewed_Lab; Tundra_Akademy | Native routing in installed 1.3.6; vessel-wide correction in prepared 1.3.7. |
| KuddleShack | moar-kuddleshack; crewCabin; ELSurveyStation; mk2CrewCabin | Vessel-wide correction already in installed 1.3.6. |

Source audit found exactly these three MoarKerbalsBase subclasses. All call ExecutePaid. Cloning's initial attempt, paid accident and optional additional births use the same payment path. Recruitment loops and multi-pair completion batches also use it. Recipe compatibility patches, including alternate life-support recipes, change resource names/amounts rather than introducing another consumer. ModuleMoarScaledCrew and MoarPassengerSeat are passive and have no recipe request path.

Version 1.3.8 enforces ALL_VESSEL in the shared RecipeResources adapter and removes the per-module boolean/virtual switch entirely. Both native Part.GetConnectedResourceTotals and Part.RequestResource calls specify ALL_VESSEL. Every consuming module inherits that path. The same Request method handles negative refund requests inside Rules.Transact. Only flow-enabled tanks on the same vessel participate; this does not access other vessels or planetary warehouses, change locked tank states, or change engines' resource routing. No recipe or price is changed.

ExecutePaid also performs the named resource/currency preflight centrally, including cloning attempts, before consuming resources. Existing action-specific readiness, crew and situation gates remain. Post-preflight failures still use the transaction error/log path and refund consumed resources where applicable.

Verification includes an inspection of the compiled DLL using System.Reflection.Metadata: require the known consuming subclasses; require every one to call the shared payment method; reject direct native resource calls outside the one adapter; reject a reintroduced per-module routing switch; and verify shared preflight/payment method references. Hosted production-module checks cover cloning, recruitment and multi-pair reproduction. Adapter/transaction checks cover vessel-wide totals, locked tanks, one payment per operation, failed transactions/refunds and the full Akademy recipe. Build/native API checks do not establish Unity's live graph or actual gameplay.

Native acceptance after installation: for each of the three ability types, use a sufficient full recipe distributed across multiple unlocked tanks, including LiquidFuel/Oxidizer in the previously excluded large tank; check one correct charge, a named shortage when insufficient, a locked tank remaining unavailable, and no payment on failed prerequisites. Repeat one failed later ingredient/crew operation to verify refunds. Confirm cloning accidents/additional births and recruitment/multi-pair batches remain individually paid and bounded by seats/resources. No saves or attachment data are manually edited by this audit.
