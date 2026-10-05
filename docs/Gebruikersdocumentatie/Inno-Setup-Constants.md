# Inno Setup-constants: wat kun je invullen in tekstvelden?

Status: kennisdocument, nog geen onderdeel van een handleiding of in-app help. Dit vat samen wat
in een aantal tekstvelden van Inno Setup Studio (en van Inno Setup zelf) ingevuld kan worden
naast gewone letterlijke tekst: een zogeheten "constant", een plaatshouder die Inno Setup pas op
het moment van installeren invult. Later kiezen we of dit een los PDF-hoofdstuk wordt (roadmap
fase 8) of dat elk relevant veld een eigen help-icoon krijgt met (een deel van) deze tekst.
Totdat dat gebeurd is, is dit document de ene plek waar deze kennis vastligt.

## Waar dit voor bedoeld is

Direct aanleiding: de drie velden op het User Info-scherm (Standaard naam, Standaard organisatie,
Standaard serienummer). Daar ligt geen vaste lijst met keuzes voor de hand, omdat de bruikbare
waarde per project en per manier van uitleveren verschilt (zie hieronder). De gebruiker moet dus
zelf kunnen intypen wat van toepassing is, inclusief een constant met eigen parameters zoals
`{reg:HKxx\Sleutel,Waarde|Standaard}`. Vandaar: een hint-tekst onder het veld, geen keuzelijst.

Andere velden in Inno Setup Studio ondersteunen dezelfde syntax, bijvoorbeeld Standaard
installatiemap (`{autopf}\<Naam applicatie>`) en Standaard startmenugroep. Dit document beperkt
zich voorlopig tot de constants die voor de User Info-velden zinvol zijn; een volledige lijst
staat onderaan ter referentie.

## Syntax

Een constant staat tussen accolades: `{naam}`. Een deel ervan neemt ook een parameter en een
terugvalwaarde, gescheiden door een `|`: `{naam:Parameter|Standaardwaarde}`. Als de parameter niet
gevonden wordt (bijvoorbeeld een registersleutel die niet bestaat, of een commandoregelparameter
die niet is meegegeven), gebruikt Setup de standaardwaarde na de `|`. Die standaardwaarde mag leeg
zijn.

Let op: dit geldt niet voor elke constant. Bij `{code:FunctieNaam|Parameter}` is de tekst na de `|`
geen terugvalwaarde maar de tekstparameter die aan de Pascal Script-functie wordt doorgegeven (laat
je hem weg, dan krijgt de functie een lege tekst). Een komma, `|` of `}` binnen een waarde moet je
met %-codering schrijven.

## De drie User Info-velden

### Standaard naam (`DefaultUserInfoName`)

Volgens de officiële Inno Setup-documentatie is de ingebouwde standaardwaarde van dit veld zelf al
`{sysuserinfoname}`: de naam waarop Windows geregistreerd staat (register:
`HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion`, waarde `RegisteredOwner`). Op recente
Windows-versies staat dat vaak leeg, dus in de praktijk is deze constant niet altijd betrouwbaar.
Een alternatief is `{username}`: de naam van het Windows-account waarmee de installatie wordt
uitgevoerd, meestal wél gevuld.

Voorbeelden:

- Leeg laten = Inno Setup gebruikt zelf `{sysuserinfoname}`.
- `{username}` = de huidige Windows-gebruikersnaam.
- `{reg:HKLM\SOFTWARE\Voortman\Licentie,Naam|}` = een naam die een eigen licentiemechanisme al in
  het register heeft gezet.

### Standaard organisatie (`DefaultUserInfoOrg`)

Zelfde opzet als Naam. Ingebouwde standaardwaarde: `{sysuserinfoorg}` (register-waarde
`RegisteredOrganization`, zelfde sleutel als hierboven).

### Standaard serienummer (`DefaultUserInfoSerial`)

Hier is er geen Windows-registervak dat van nature een serienummer bijhoudt, dus deze directive
heeft geen ingebouwde standaardwaarde: zonder eigen invulling is het veld leeg. Twee constants die
hier praktisch zijn:

- `{reg:HKxx\Sleutel,Waarde|Standaard}` - leest een waarde uit het register. Bruikbaar als een
  eerdere installatie, of een apart licentiehulpmiddel, de sleutel al ergens heeft neergezet.
- `{param:Naam|Standaard}` - leest een commandoregelparameter. Bruikbaar voor silent/unattended
  installaties: een deploymentscript start de installer dan met bijvoorbeeld `/SERIAL=ABC123`, en
  dat komt hier automatisch in te staan.
- `{ini:Bestand,Sectie,Sleutel|Standaard}` - leest een waarde uit een .ini-bestand naast setup.exe.
  Praktisch bij een per-klant uitgeleverde installer met een eigen ini-bestand erbij.


## Hoe dit samenwerkt met "Onthoud ingevulde gegevens bij een update"

Het vinkje `UsePreviousUserInfo` (standaard aan, net als in Inno Setup zelf) zorgt ervoor dat
Setup bij het opstarten in het register kijkt of dezelfde applicatie al eerder geïnstalleerd is.
Zo ja, dan gebruikt Setup de eerder ingevoerde naam, organisatie én serienummer als
standaardwaarden op deze pagina, in plaats van de waarden uit de drie velden hierboven. De drie
velden (en de constants erin) doen dus vooral iets bij een eerste installatie, of wanneer er geen
spoor van een eerdere installatie gevonden wordt. Bij een update met dit vinkje aan wint de eerder
ingevoerde waarde automatisch.

## Het serienummerveld zichtbaar maken

Standaard toont Setup het Serienummer-veld op deze pagina helemaal niet. Het verschijnt pas zodra
het `[Code]`-gedeelte van het project een `CheckSerial`-functie bevat:

```pascal
function CheckSerial(Serial: String): Boolean;
begin
  Result := True; { True = serienummer geaccepteerd, False = geweigerd }
end;
```

Setup roept deze functie aan zodra de gebruiker op Volgende klikt, met het ingevoerde serienummer
als parameter. Geef je geen `CheckSerial`-functie mee, dan blijft het veld onzichtbaar, ook als
`DefaultUserInfoSerial` wel een waarde heeft. Let op: dit is geen beveiliging tegen misbruik, de
Inno Setup-documentatie zelf waarschuwt dat de controle (zonder encryptie, met vrij beschikbare
broncode) relatief eenvoudig te omzeilen is. Gebruik het dus als gemak voor de eindgebruiker, en
controleer het ingevoerde serienummer (beschikbaar via de constant `{userinfoserial}`) nog eens in
de eigen applicatie. Dit scherm bestaat in Inno Setup Studio nog als los tekstveld; de
`[Code]`-sectie en de `CheckSerial`-snippet zelf horen bij fase 6 (Pascal-code-editor), nog niet
gebouwd.

## Overige constants (volledigheid)

Deze lijst is niet uitputtend en gericht op wat elders in een .issproj-bestand bruikbaar is, niet
per se op de User Info-velden hierboven.

| Constant | Betekenis |
|---|---|
| `{app}` | De installatiemap, zoals gekozen door de gebruiker. |
| `{src}` | De map waarin de Setup-bestanden staan. |
| `{group}` / `{groupname}` | De gekozen Start Menu-map, respectievelijk alleen de naam ervan. |
| `{autopf}` | Program Files (32- of 64-bit, afhankelijk van installatiemodus). |
| `{userappdata}` / `{commonappdata}` | Application Data-map, voor de huidige gebruiker resp. alle gebruikers. |
| `{computername}` | De naam van de computer. |
| `{username}` | De naam van het Windows-account dat Setup uitvoert. |
| `{sysuserinfoname}` / `{sysuserinfoorg}` | Naam/organisatie waarop Windows geregistreerd staat. |
| `{userinfoname}` / `{userinfoorg}` / `{userinfoserial}` | Wat de gebruiker zelf invulde op dit scherm (bruikbaar in andere secties, bijvoorbeeld `[Registry]`, niet als standaardwaarde van hetzelfde veld). |
| `{reg:HKxx\Sleutel,Waarde\|Standaard}` | Een waarde uit het register. |
| `{param:Naam\|Standaard}` | Een commandoregelparameter waarmee Setup is gestart. |
| `{%OMGEVINGSVARIABELE\|Standaard}` | Een omgevingsvariabele. |
| `{ini:Bestand,Sectie,Sleutel\|Standaard}` | Een waarde uit een .ini-bestand. |
| `{cm:BerichtNaam}` | Een eigen vertaalbaar bericht (`[CustomMessages]`), afhankelijk van de gekozen taal. |
| `{code:FunctieNaam\|Parameter}` | De returnwaarde van een eigen Pascal Script-functie (fase 6). Wat na de `\|` staat is de parameter voor die functie, geen terugvalwaarde. |

## Bronnen

- [DefaultUserInfoName](https://jrsoftware.org/ishelp/topic_setup_defaultuserinfoname.htm)
- [DefaultUserInfoOrg](https://jrsoftware.org/ishelp/topic_setup_defaultuserinfoorg.htm)
- [DefaultUserInfoSerial](https://jrsoftware.org/ishelp/topic_setup_defaultuserinfoserial.htm)
- [UsePreviousUserInfo](https://jrsoftware.org/ishelp/topic_setup_useprevioususerinfo.htm)
- [CheckSerial event function (Pascal Scripting: Event Functions)](https://jrsoftware.org/ishelp/topic_scriptevents.htm)
- [Constants-referentie](https://jrsoftware.org/ishelp/topic_consts.htm)

Geverifieerd tegen de officiële documentatie op 2026-10-02, naar aanleiding van Herberts vraag
over het User Info-scherm.
