# Ontwerp: vertalingen van knopteksten via het Standaardscherm

Datum: 2026-10-06. Status: goedgekeurd door Herbert en gebouwd in PR #32.

## 1. Aanleiding

Herbert testte PR #31 en zag dat het Standaardscherm de knopteksten en tooltips wel kent, maar geen vertalingen per taal. Hij kan daar alleen de Engelse (universele) tekst invullen. In een meertalig project moet hij daardoor op elk van de acht schermen dezelfde vertaling opnieuw invullen.

Dit is geen fout maar een keuze uit PR #21: een vertaling op het Standaardscherm zou toen niets doen, dus zijn de vertaalrijen daar verborgen (`NoLanguageOverridesOnDefaultScreen`) en leest `ButtonSettingsResolver` vertalingen alleen van het scherm zelf. Het punt staat sinds PR #21 op de backlog ("cascade van vertalingen via het Standaardscherm"). Dit ontwerp pakt het op.

## 2. Besluit

De cascade geldt per knop, per taal, en voor tekst en tooltip afzonderlijk. Voor één taal geldt de eerste regel die van toepassing is:

1. Het scherm heeft een eigen vertaling voor die taal: die geldt.
2. Het scherm heeft zelf een tekst ingevuld (de Engelse tekst, zonder vertaling voor die taal): die eigen tekst geldt voor die taal. Het scherm overschrijft het Standaardscherm voor alle talen, behalve waar het een eigen vertaling heeft. Herbert heeft dit op 2026-10-06 bevestigd.
3. Het scherm heeft niets ingevuld: de vertaling van het Standaardscherm voor die taal geldt. Is die er niet, dan geldt de Engelse tekst van het Standaardscherm. Is die er ook niet, dan houdt Setup zijn eigen tekst.

Een tekst die alleen uit spaties bestaat, telt overal als leeg (zoals nu).

Voorbeeld, project met Nederlands en Duits, knop Volgende:

| Standaardscherm | Scherm Licentie | Engels | Nederlands | Duits |
|---|---|---|---|---|
| tekst "Verder", nl "Doorgaan" | niets ingevuld | Verder | Doorgaan | Verder |
| tekst "Verder", nl "Doorgaan" | tekst "Akkoord" | Akkoord | Akkoord | Akkoord |
| tekst "Verder", nl "Doorgaan" | tekst "Akkoord", nl "Ja" | Akkoord | Ja | Akkoord |
| tekst "Verder", nl "Doorgaan" | nl "Ja" | Verder | Ja | Verder |

Duits krijgt in de eerste rij "Verder": het Standaardscherm heeft geen Duitse vertaling, dus geldt zijn Engelse tekst. In de laatste rij heeft het scherm geen eigen tekst, dus het Standaardscherm geeft Engels en Duits.

Verworpen: de vertaling van het Standaardscherm laten winnen van een eigen Engelse tekst op het scherm. Dan zou de Nederlandse gebruiker "Doorgaan" zien waar de Engelse gebruiker "Akkoord" ziet, en dat zijn twee verschillende knoppen.

Geen cascade voor de twee Bladeren-knoppen. Ze hebben geen Standaardscherm-laag en dat blijft zo.

## 3. Wat verandert

**Core.** `ButtonSettingsResolver.Resolve` bepaalt de twee vertalingenlijsten met de regels hierboven, uit de lijsten van het scherm en van het Standaardscherm. De regel zit in één kleine publieke methode, zodat de editor dezelfde regel kan gebruiken voor zijn voorinvulling (zie hieronder) en de regel niet op twee plaatsen staat.

**Generator.** `ButtonScript` verandert niet. Die schrijft de vertalingen uit het resolverresultaat al weg, inclusief het geval dat de universele tekst leeg is en er wel een vertaling bestaat (lege regel plus een beveiligde toewijzing). Alleen de commentaren die zeggen dat vertalingen niet cascaderen worden aangepast. Een regeleinde in een vertaling van het Standaardscherm wordt al één keer gemeld onder `DefaultScreenButtons`.

**Editor.**

- Het venster Knopeigenschappen toont voor de knoppen van het Standaardscherm de vertaalrijen. `NoLanguageOverridesOnDefaultScreen` vervalt.
- Op een gewoon scherm toont een lege vertaalrij als voorinvulling (grijze tekst) wat er dan geldt: de eigen tekst van het scherm zoals die nu in het venster staat, anders de vertaling van het Standaardscherm voor die taal, anders de Engelse tekst van het Standaardscherm. Zo zie je zonder te rekenen wat de gebruiker in die taal krijgt. De voorinvulling past zich aan terwijl je de tekst bovenin wijzigt.
- De hint onder "Vertalingen per taal" krijgt een tweede zin voor gewone schermen: "Staat hierboven niets, dan geldt de vertaling van het Standaardscherm, anders de tekst van het Standaardscherm, anders de eigen tekst van Setup." Op het Standaardscherm blijft alleen de eerste zin staan. Dat is één nieuwe tekstsleutel (`HintLanguageOverridesFromDefaultScreen`) naast de bestaande hint, in NL, EN en DE.
- `Save` blijft de vertalingen samenvoegen met de oorspronkelijke lijst (CodeRabbit-fix uit PR #21), dus talen die niet als rij zichtbaar zijn blijven behouden.
- Het projectformaat verandert niet: de lijsten van het Standaardscherm bestonden al in het model en in het JSON-bestand, ze waren alleen niet via de IDE te vullen.

## 4. Tests

- Resolver: eigen vertaling wint van die van het Standaardscherm; eigen tekst zonder vertaling blokkeert de vertaling van het Standaardscherm; niets ingevuld op het scherm geeft de vertaling van het Standaardscherm; tekst en tooltip cascaderen apart; lege en alleen-spaties-vertalingen tellen niet; de Bladeren-knop cascadeert niet; de vier rijen uit het voorbeeld hierboven als testgevallen.
- Generator: de bestaande test `Translations_do_not_cascade_from_the_default_screen` wordt vervangen door tests voor de nieuwe regels; een vertaling van het Standaardscherm komt op elk getoond scherm terecht; lege universele tekst met alleen een vertaling van het Standaardscherm geeft een beveiligde toewijzing; scherm dat uit staat krijgt niets.
- ISCC: een Standaardscherm met vertalingen met speciale tekens compileert zonder waarschuwing.
- Het goldenbestand `Buttons.iss` is niet veranderd: het Standaardscherm heeft daarin geen vertalingen.
- De editor heeft geen eenheidstests (het testproject verwijst alleen naar Core), dus dat gedeelte test Herbert met de lijst hieronder.

## 5. Handmatige testpunten voor Herbert

- Project met Nederlands en Duits. Standaardscherm, knop Volgende: vul tekst, Nederlandse vertaling en Duitse vertaling in. Het venster toont de vertaalrijen.
- Gewoon scherm zonder eigen tekst: de lege vertaalrijen tonen de vertaling van het Standaardscherm als grijze tekst.
- Typ op dat scherm een eigen tekst bovenin: de grijze tekst in beide rijen wordt die eigen tekst.
- Vul op dat scherm een eigen Nederlandse vertaling in: alleen de Nederlandse rij verandert.
- Genereer het `.iss`, compileer en draai de installer met `/LANG=dutch` en `/LANG=german`: elk scherm toont wat de tabel in sectie 2 voorschrijft.
- Een taal die je uit het project haalt: de vertalingen voor die taal blijven in het projectbestand staan en komen terug zodra je de taal weer aanzet.

## 6. Documentatie

Bij de bouw aan te passen, met spiegel naar Obsidian: `Ontwerp-Knopinstellingen-Generator.md` (sectie 5 regel over vertalingen, sectie 9 en 13), `Architectuur-en-Ontwerp.md` (nieuwe sectie 34 en de backlogregel in sectie 24), `Feature-Checklist.md` (meertalige knopteksten, Standaardscherm).

## 7. Buiten deze wijziging

- Een eigen tekst per taal op het Standaardscherm voor de Bladeren-knoppen: die hebben geen Standaardscherm-laag.
- Setup's eigen standaardteksten per taal als voorinvulling. De app kent die niet per taal, dus een rij zonder eigen of geërfde tekst blijft leeg.
