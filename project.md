Tehnicki zahtevi:
Patterns: Repository pattern, Unit of Work, AutoMapper za mapiranje DTO ↔ Entity ↔ ViewModel
Arhitektura: Clean Architecture ili Vertical Slice Architecture (po izboru studenta, ali konzistentno
kroz ceo projekat)
CQRS pattern: obavezno (preporuka: MediatR), Commands i Queries jasno razdvojeni
Autentifikacija: JWT access token + Refresh token (sa rotacijom i revoke listom)
Validacija: FluentValidation ili neki drugi
Logovanje: Serilog (strukturirani logovi, sink po izboru — fajl, Seq ili konzola) ili neki drugi
Globalni exception handler i konzistentan format greške (Problem Details — RFC 7807)
Dokumentacija: Swagger/OpenAPI sa primerima request-a i response-a
Verzionisanje API-ja
README sa uputstvom za pokretanje, ER/dijagramom modela i opisom arhitekture
Custom middleware: svaki projekat ima drugačiji custom middleware

Projekat — PetHaven: Platforma za udomljavanje ljubimaca
Opis i ideja
Platforma koja povezuje skloništa za životinje, fostere i potencijalne usvojitelje. Skloništa objavljuju
životinje sa detaljnim profilima (zdravlje, ponašanje, energija, slaganje sa decom/drugim životinjama), a
usvojitelji popunjavaju aplikaciju koja prolazi kroz proces provere. Platforma takođe podržava prijavu
izgubljenih i pronađenih životinja, donacije skloništima i volonterske prijave.
Topla, intuitivna tema sa stvarnom društvenom korišću; pokriva tok aplikacije sa više koraka i upravljanje
fajlovima.
Domen i ključne funkcionalnosti
Profili i uloge - Uloge: Adopter , Shelter , Foster , Admin - Adopter profil: kontakt, adresa, tip stana
(kuća/stan), broj članova domaćinstva, deca, druge životinje, iskustvo, razlog za usvajanje - Sklonište
profil: naziv, lokacija, kontakt, opis, foto galerija, verifikovano (admin proverava dokumenta) - Foster
profil: dostupnost, tip životinje koju prima, kapacitet
Životinje - Životinja: vrsta, rasa, ime, godine (ili procena), pol, veličina, boja, težina, opis, više fotografija,
video - Zdravstveni karton: vakcinacije, sterilizacija, mikročip, hronične bolesti, lekovi - Ponašanje:
energija, kompatibilnost sa decom/psima/mačkama, posebne potrebe, dosadašnje ponašanje - Status:
Available , InAdoptionProcess , Adopted , InFosterCare , Deceased - Sklonište koje brine o
životinji, datum prijema
Pretraga - Filteri: vrsta, rasa, godine, veličina, energija, kompatibilnost - Geografski filter — radijus od
korisnika ili izabrani grad - Sortiranje po datumu prijema (najduže u skloništu prvo), datumu objave -
Spasimo posebne potrebe — istaknuta sekcija za životinje koje teže pronalaze dom
Proces usvajanja - Aplikacija sa pitanjima specifičnim za vrstu/rasu - Tok: Submitted → UnderReview
→ InterviewScheduled → HomeVisitScheduled → Approved / Rejected → Adopted - Sklonište
ostavlja interne beleške po aplikaciji, traži dodatne dokumente - Generisanje ugovora o usvajanju (PDF)
sa podacima usvojitelja i životinje - Kontrola posle usvajanja — sklonište može tražiti foto/izveštaj nakon
30/90 dana
Foster program - Foster prijava sa periodom dostupnosti - Sklonište povezuje životinju sa foster
osobom - Foster ostavlja izveštaje o ponašanju i napretku životinje
Izgubljeni i pronađeni - Prijava izgubljene životinje (lokacija, datum, opis, slika, kontakt) - Prijava
pronađene životinje - Sistem traži potencijalna poklapanja po opisu i lokaciji
Donacije i volontiranje - Donaciona stranica skloništa sa ciljevima (hrana, lekovi, oprema) - Praćenje
donacija (mock payment) - Volonterska prijava sa preferiranim aktivnostima i terminima
Notifikacije - Nova životinja koja zadovoljava sačuvane filtere, promena statusa aplikacije, podsetnici o
post-adopcijskom izveštaju
Admin - Verifikacija skloništa (provera dokumenata), statistike, moderacija sadržaja

Baza podataka
Relaciona — PostgreSQL ili SQL Server. Razlog: jasne veze (sklonište–životinja–aplikacija–usvojitelj),
složeni tok aplikacije sa stanjima, generisanje ugovora i statistika.
Custom middleware za ovaj projekat
Geo-location enrichment middleware — za svaki request resolvuje IP adresu klijenta u zemlju, region i
grad (preko MaxMind GeoLite2 baze ili sličnog izvora) i upisuje to u HttpContext.Items kao
RequestGeo . Aplikacija to koristi za "skloništa blizu vas" na početnoj strani bez potrebe da korisnik
ručno bira grad, kao i za detekciju nedoslednosti (npr. nalog se obično prijavljuje iz Beograda, sad iz
inostranstva → upozorenje).

Nastavi rad na mom backend projektu PetHaven – platforma za udomljavanje ljubimaca.

Projekat treba biti urađen u .NET / ASP.NET Core Web API sa relacionom bazom PostgreSQL .

Molim te da nastaviš projekat profesionalno, sa jasnom folder strukturom, čistim kodom i komentarima gde je potrebno

#FRONTEND FAZA
Sada kada je backend završen, želim da napraviš kompletan frontend za PetHaven.

ZAHTEVI:

- Kreiraj novi folder "frontend" u root direktorijumu projekta.
- Koristi React + Vite.
- Koristi React Router.
- Koristi Axios za komunikaciju sa backend API-jem.
- Napravi modern UI koristeći Tailwind CSS.
- Frontend mora biti potpuno povezan sa postojećim backend endpointima.
  TEHNIČKI ZAHTEVI:

- Kreiraj frontend kao poseban projekat u folderu frontend.
- Organizuj kod po feature-based arhitekturi.
- Kreiraj reusable komponente.
- Implementiraj JWT autentifikaciju.
- Implementiraj protected routes.
- Dodaj loading i error handling.
- Dodaj .env konfiguraciju.
- Generiši sve stranice, komponente i servise.
- Prati postojeću backend arhitekturu i DTO modele.

Pre početka analiziraj postojeći backend i mapiraj sve API rute na frontend funkcionalnosti.
