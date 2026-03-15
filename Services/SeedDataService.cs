using TreninkovyPlanovac.Models;

namespace TreninkovyPlanovac.Services;

public class SeedDataService
{
    private readonly DatabaseService _db;

    public SeedDataService(DatabaseService db)
    {
        _db = db;
    }

    public async Task SeedAsync()
    {
        var existujici = await _db.GetCviceniAsync();
        if (existujici.Count > 0)
            return; // už máme data

        var cviky = GetVsechnyCviky();
        foreach (var cvik in cviky)
        {
            await _db.UlozCviceniAsync(cvik);
        }
    }

    private List<Cviceni> GetVsechnyCviky()
    {
        var cviky = new List<Cviceni>();

        // === HRUDNÍK ===
        cviky.AddRange(CreateCategory("Hrudník", new[]
        {
            ("Bench press (rovná lavička)", "Tlak velké činky na rovné lavičce"),
            ("Bench press šikmá lavička", "Tlak velké činky na šikmé lavičce (incline)"),
            ("Bench press klesající lavička", "Tlak velké činky na klesající lavičce (decline)"),
            ("Tlak jednoručkami na rovné lavičce", "Tlak jednoručkami vleže na rovné lavičce"),
            ("Tlak jednoručkami šikmá lavička", "Tlak jednoručkami na šikmé lavičce"),
            ("Kliky", "Kliky na zemi — klasické provedení"),
            ("Kliky úzké", "Kliky s úzkým postavením rukou"),
            ("Kliky široké", "Kliky se širokým postavením rukou"),
            ("Kliky na šikmé ploše", "Nohy výše než ruce — důraz na horní hrudník"),
            ("Rozpažování jednoručkami", "Dumbbell flyes na rovné lavičce"),
            ("Rozpažování jednoručkami šikmá", "Dumbbell flyes na šikmé lavičce"),
            ("Rozpažování na kladce", "Cable flyes — stojící rozpažování"),
            ("Pec deck", "Stroj na hrudník — rozpažování"),
            ("Tlak na stroji (chest press)", "Machine chest press"),
        }));

        // === ZÁDA ===
        cviky.AddRange(CreateCategory("Záda", new[]
        {
            ("Shyby nadhmatem", "Pull-ups — široký úchop nadhmatem"),
            ("Shyby podhmatem", "Chin-ups — úzký úchop podhmatem"),
            ("Mrtvý tah", "Deadlift — základní compound cvik"),
            ("Rumunský mrtvý tah", "Romanian deadlift — důraz na záda a hamstringy"),
            ("Veslování velkou činkou", "Barbell row v předklonu"),
            ("Veslování jednoručkou", "Dumbbell row — jedna ruka na lavičce"),
            ("Veslování T-osy", "T-bar row"),
            ("Přítah horní kladky k hrudníku", "Lat pulldown — široký úchop"),
            ("Přítah horní kladky podhmatem", "Lat pulldown — úzký podhmat"),
            ("Přítah vodorovné kladky", "Seated cable row"),
            ("Hyperextenze", "Zvedání trupu z hyperextenzní lavice"),
            ("Reverse pec deck", "Pec deck vzad — zadní delty a horní záda"),
            ("Pullover jednoručkou", "Dumbbell pullover na lavičce"),
        }));

        // === RAMENA ===
        cviky.AddRange(CreateCategory("Ramena", new[]
        {
            ("Tlak velkou činkou nad hlavu", "Overhead press / military press"),
            ("Tlak jednoručkami nad hlavu", "Dumbbell shoulder press vsedě"),
            ("Tlak na stroji nad hlavu", "Machine shoulder press"),
            ("Arnoldův tlak", "Arnold press — rotační tlak jednoručkami"),
            ("Upažování do stran", "Lateral raises s jednoručkami"),
            ("Upažování do stran na kladce", "Cable lateral raises"),
            ("Předpažování", "Front raises s jednoručkami"),
            ("Předpažování s kotoučem", "Front raise s kotoučem oběma rukama"),
            ("Přítahy činky k bradě", "Upright rows"),
            ("Reverse pec deck (zadní delty)", "Pec deck vzad pro zadní ramena"),
            ("Předklony s jednoručkami (zadní delty)", "Bent-over reverse flyes"),
            ("Face pulls na kladce", "Face pulls — zadní delty a rotátory"),
            ("Shrugs s jednoručkami", "Zvedání ramen — trapézy"),
            ("Shrugs s velkou činkou", "Zvedání ramen s velkou činkou"),
        }));

        // === BICEPS ===
        cviky.AddRange(CreateCategory("Biceps", new[]
        {
            ("Bicepsový zdvih velkou činkou", "Barbell curl — základní cvik na biceps"),
            ("Bicepsový zdvih s EZ činkou", "EZ bar curl — šetrnější pro zápěstí"),
            ("Bicepsový zdvih jednoručkami", "Dumbbell curl — střídavě nebo současně"),
            ("Bicepsový zdvih jednoručkami vsedě", "Incline dumbbell curl na šikmé lavičce"),
            ("Kladívkový zdvih", "Hammer curl — neutrální úchop"),
            ("Zdvih na Scottově lavičce", "Preacher curl — izolace bicepsu"),
            ("Soustředěný zdvih", "Concentration curl vsedě"),
            ("Zdvih na kladce", "Cable curl — spodní kladka"),
            ("Zdvih na stroji", "Machine biceps curl"),
            ("Cross-body hammer curl", "Kladívkový zdvih přes tělo"),
        }));

        // === TRICEPS ===
        cviky.AddRange(CreateCategory("Triceps", new[]
        {
            ("Stahování horní kladky lanem", "Triceps rope pushdown"),
            ("Stahování horní kladky tyčí", "Triceps bar pushdown"),
            ("Bench press úzký úchop", "Close-grip bench press"),
            ("Francouzský tlak (skull crushers)", "Lying triceps extension s EZ činkou"),
            ("Overhead extension jednoručkou", "Single-arm overhead triceps extension"),
            ("Overhead extension oběma rukama", "Overhead extension s jednoručkou oběma rukama"),
            ("Kickback jednoručkou", "Triceps kickback v předklonu"),
            ("Dipy", "Dips na bradlech — důraz na triceps"),
            ("Dipy na lavičce", "Bench dips — nohy na zemi nebo na lavičce"),
            ("Triceps na stroji", "Machine triceps press"),
            ("Diamond kliky", "Kliky s rukama u sebe — diamantové"),
        }));

        // === PŘEDNÍ STEHNA ===
        cviky.AddRange(CreateCategory("Přední stehna", new[]
        {
            ("Dřep s velkou činkou", "Back squat — činka na zádech"),
            ("Přední dřep", "Front squat — činka na ramenou vpředu"),
            ("Dřep sumo", "Sumo squat — široký postoj"),
            ("Goblet squat", "Dřep s jednoručkou / kettlebellem u hrudi"),
            ("Bulharský dřep", "Bulgarian split squat — zadní noha na lavičce"),
            ("Výpady dopředu", "Forward lunges s jednoručkami"),
            ("Výpady vzad", "Reverse lunges"),
            ("Chůze s výpady", "Walking lunges"),
            ("Leg press", "Tlak nohama na stroji"),
            ("Hack squat", "Hack squat na stroji"),
            ("Předkopávání na stroji", "Leg extensions — izolace quadricepsů"),
            ("Dřepy ve Smithově stroji", "Smith machine squats"),
            ("Sissy squat", "Sissy squat — izolace předních stehen"),
        }));

        // === ZADNÍ STEHNA ===
        cviky.AddRange(CreateCategory("Zadní stehna", new[]
        {
            ("Mrtvý tah (pro hamstringy)", "Deadlift s důrazem na zadní stehna"),
            ("Rumunský mrtvý tah", "Romanian deadlift — hlavní cvik na hamstringy"),
            ("Rumunský mrtvý tah jednoručkami", "RDL s jednoručkami"),
            ("Zakopávání vleže", "Lying leg curl na stroji"),
            ("Zakopávání vsedě", "Seated leg curl na stroji"),
            ("Zakopávání vestoje", "Standing leg curl na stroji"),
            ("Nordic hamstring curl", "Nordic curl — pokročilé cvičení s vlastní vahou"),
            ("Good morning", "Předklon s činkou na zádech"),
            ("Hyperextenze (hamstringy)", "Hyperextenze s důrazem na hamstringy"),
        }));

        // === HÝŽDĚ ===
        cviky.AddRange(CreateCategory("Hýždě", new[]
        {
            ("Hip thrust s velkou činkou", "Barbell hip thrust — hlavní cvik na hýždě"),
            ("Hip thrust jednoručkou", "Hip thrust s jednoručkou na klíně"),
            ("Glute bridge", "Most — zvedání pánve vleže na zádech"),
            ("Glute bridge jednonožní", "Single-leg glute bridge"),
            ("Abdukce na stroji", "Hip abduction machine"),
            ("Abdukce na kladce", "Cable hip abduction"),
            ("Kickback na kladce", "Cable glute kickback"),
            ("Step-up na lavičku", "Výstupy na lavičku s jednoručkami"),
            ("Sumo dřep (hýždě)", "Sumo squat s důrazem na hýždě"),
            ("Frog pumps", "Pumpování v pozici žáby — aktivace hýždí"),
        }));

        // === LÝTKA ===
        cviky.AddRange(CreateCategory("Lýtka", new[]
        {
            ("Výpony vestoje", "Standing calf raise — důraz na gastrocnemius"),
            ("Výpony vsedě", "Seated calf raise — důraz na soleus"),
            ("Výpony na leg pressu", "Calf raise na leg press stroji"),
            ("Výpony ve Smithově stroji", "Smith machine calf raise"),
            ("Výpony na jedné noze", "Single-leg calf raise"),
            ("Donkey calf raise", "Výpony v předklonu se závažím"),
        }));

        // === CORE / BŘICHO ===
        cviky.AddRange(CreateCategory("Core", new[]
        {
            ("Plank (deska)", "Výdrž v opoře na předloktích"),
            ("Boční plank", "Side plank — výdrž na boku"),
            ("Sklapovačky", "Crunches — základní cvik na břicho"),
            ("Sklapovačky na kladce", "Cable crunch — vkleče u kladky"),
            ("Sklapovačky na klesající lavičce", "Decline sit-ups"),
            ("Ruský twist", "Russian twist — rotace trupu vsedě"),
            ("Zvedání nohou ve visu", "Hanging leg raise na hrazdě"),
            ("Zvedání nohou vleže", "Lying leg raise na zemi"),
            ("Zvedání kolen ve visu", "Hanging knee raise"),
            ("Nůžky", "Střídavé zvedání nohou vleže"),
            ("Cyklická sklapovačka", "Bicycle crunch — koleno k loktu"),
            ("Ab wheel rollout", "Kolečko na břicho — výjezd vpřed"),
            ("Mountain climbers", "Horolezci — v opoře střídavé přitahování kolen"),
            ("Dead bug", "Mrtvý brouček — střídavé natahování končetin"),
            ("Pallof press na kladce", "Anti-rotační cvičení na kladce"),
            ("Wood chop na kladce", "Sekání — rotace trupu na kladce"),
            ("Hollow hold", "Dutá pozice — výdrž na zádech"),
        }));

        // === PŘEDLOKTÍ ===
        cviky.AddRange(CreateCategory("Předloktí", new[]
        {
            ("Flexe zápěstí s činkou", "Wrist curls — předloktí na kolenou"),
            ("Inverzní flexe zápěstí", "Reverse wrist curls"),
            ("Farmářská chůze", "Farmer's carry — chůze s těžkými činkami"),
            ("Vis na hrazdě", "Dead hang — výdrž ve visu"),
        }));

        // === KARDIO ===
        cviky.AddRange(CreateCategory("Kardio", new[]
        {
            ("Běh", "Běh venku nebo na pásu"),
            ("Rychlá chůze", "Chůze vyšší intenzitou / do kopce"),
            ("Jízda na kole / rotoped", "Cyklistika nebo stacionární kolo"),
            ("Eliptický trenažér", "Eliptical — nízkonárazové kardio"),
            ("Veslařský trenažér", "Rowing machine"),
            ("Plavání", "Plavání — celotělové kardio"),
            ("Švihadlo", "Skákání přes švihadlo"),
            ("Sprinty", "Intervalové sprinty"),
            ("Burpees", "Burpee — celotělový výbušný cvik"),
            ("Box jumps", "Skoky na bednu"),
            ("Jump squats", "Výskoky z dřepu"),
            ("Step-ups", "Vykročování na step / lavičku"),
            ("High knees", "Běh na místě s vysokými koleny"),
        }));

        return cviky;
    }

    private static List<Cviceni> CreateCategory(string kategorie, (string nazev, string popis)[] cviky)
    {
        return cviky.Select(c => new Cviceni
        {
            Nazev = c.nazev,
            Popis = c.popis,
            Kategorie = kategorie
        }).ToList();
    }
}
