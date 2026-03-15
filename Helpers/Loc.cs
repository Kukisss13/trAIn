using System.ComponentModel;
using System.Globalization;

namespace TreninkovyPlanovac.Helpers;

/// <summary>
/// Localization singleton — provides translations for CZ/EN with runtime switching.
/// XAML usage: Text="{Binding [Key], Source={x:Static h:Loc.Instance}}"
/// C# usage:  Loc.T("Key")
/// </summary>
public class Loc : INotifyPropertyChanged
{
    public static Loc Instance { get; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    private string _language = "cs";
    public string Language => _language;

    /// <summary>Indexer for XAML bindings</summary>
    public string this[string key] => Get(key);

    /// <summary>Static shorthand for C# code</summary>
    public static string T(string key) => Instance.Get(key);

    public string Get(string key)
    {
        var dict = _language == "en" ? _en : _cs;
        return dict.TryGetValue(key, out var val) ? val : key;
    }

    public void SetLanguage(string lang)
    {
        if (_language == lang) return;
        _language = lang;
        Preferences.Set("app_language", lang);
        CultureInfo.CurrentUICulture = new CultureInfo(lang);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Language)));
    }

    public void LoadSaved()
    {
        var saved = Preferences.Get("app_language", "cs");
        _language = saved;
        CultureInfo.CurrentUICulture = new CultureInfo(saved);
    }

    /// <summary>Re-fires PropertyChanged so all XAML bindings re-evaluate (call in OnAppearing).</summary>
    public void RefreshBindings()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
    }

    // =========================================================
    //  CZECH TRANSLATIONS (default)
    // =========================================================
    private static readonly Dictionary<string, string> _cs = new()
    {
        // === Common ===
        ["OK"] = "OK",
        ["Cancel"] = "Zrušit",
        ["Delete"] = "Smazat",
        ["Save"] = "Uložit",
        ["Error"] = "Chyba",
        ["Saved"] = "Uloženo",
        ["Loading"] = "Načítám...",
        ["Continue"] = "Pokračovat",
        ["Back"] = "Zpět",

        // === MainPage ===
        ["MainSubtitle"] = "Tvé tréninky na jednom místě",
        ["MyJourney"] = "Moje cesta",
        ["MyJourneyDesc"] = "Tvoje statistiky a progres",
        ["MyWorkouts"] = "Moje tréninky",
        ["MyWorkoutsDesc"] = "Vytvoř si trénink a spravuj své plány",
        ["StartWorkout"] = "Začít workout",
        ["StartWorkoutDesc"] = "Vyber sport a sleduj výkon",

        // === NastaveniPage ===
        ["Settings"] = "Nastavení",
        ["NoAccount"] = "Nemáš účet",
        ["SignInToSync"] = "Přihlas se pro synchronizaci tréninků",
        ["Email"] = "E-mail",
        ["RegisterOrSignIn"] = "Registrace nebo přihlášení",
        ["Phone"] = "Telefon",
        ["SignInViaSms"] = "Přihlášení přes SMS kód",
        ["AppleId"] = "Apple ID",
        ["QuickSignInApple"] = "Rychlé přihlášení přes Apple",
        ["SignOut"] = "Odhlásit se",
        ["SignOutConfirm"] = "Opravdu se chceš odhlásit?",
        ["SignedVia"] = "Přihlášeno přes {0}",
        ["Exercises"] = "Cviky",
        ["ExerciseLibrary"] = "Zásoba cviků",
        ["ExerciseLibraryDesc"] = "Prohlédni a uprav databázi cviků",
        ["AiTrainer"] = "AI Trenér",
        ["GeminiApiKey"] = "Gemini API klíč (zdarma)",
        ["SaveApiKey"] = "Uložit API klíč",
        ["KeyNotSet"] = "Klíč není nastaven",
        ["KeySet"] = "✅ Klíč nastaven ({0})",
        ["EnterApiKey"] = "Zadej API klíč.",
        ["ApiKeySaved"] = "API klíč byl uložen.",
        ["Appearance"] = "Vzhled",
        ["DarkMode"] = "Tmavý režim",
        ["LightMode"] = "Světlý režim",
        ["Active"] = "Aktivní",
        ["Profile"] = "Profil",
        ["Name"] = "Jméno",
        ["YourName"] = "Tvoje jméno",
        ["Age"] = "Věk",
        ["Gender"] = "Pohlaví",
        ["Male"] = "Muž",
        ["Female"] = "Žena",
        ["WeightKg"] = "Váha (kg)",
        ["HeightCm"] = "Výška (cm)",
        ["YourGoal"] = "Tvůj cíl",
        ["BuildMuscle"] = "Nabrat svaly",
        ["LoseWeight"] = "Zhubnout",
        ["Maintain"] = "Udržovat",
        ["Fitness"] = "Kondice",
        ["Level"] = "Úroveň",
        ["Beginner"] = "Začátečník",
        ["Intermediate"] = "Pokročilý",
        ["Expert"] = "Expert",
        ["Underweight"] = "Podváha",
        ["Normal"] = "Normální",
        ["Overweight"] = "Nadváha",
        ["Obese"] = "Obezita",
        ["SaveProfile"] = "Uložit profil",
        ["ProfileSaved"] = "Profil byl uložen.",
        ["Language"] = "Jazyk",
        ["Czech"] = "Čeština",
        ["English"] = "English",
        ["LanguageDesc"] = "Jazyk aplikace",

        // === TreninkyPage ===
        ["Workouts"] = "Tréninky",
        ["Plans"] = "Plány",
        ["CreateNew"] = "+ Vytvořit",
        ["CreateNewPlan"] = "Vytvořit nový plán",
        ["NamePlanDesc"] = "Pojmenuj plán, vyber datum a jdeme na to",
        ["PlanNamePlaceholder"] = "Název plánu (např. Push day, Nohy...)",
        ["CreatePlan"] = "Vytvořit plán",
        ["NoWorkouts"] = "Žádné tréninky",
        ["WorkoutsFromPlans"] = "Tréninky se vytvoří automaticky z plánů",
        ["NoPlans"] = "Žádné plány",
        ["CreateFirstPlan"] = "Vytvoř svůj první tréninkový plán",
        ["NewPlan"] = "Nový plán",
        ["EnterPlanName"] = "Zadej název plánu",
        ["DeletePlan"] = "Smazat plán",
        ["DeletePlanConfirm"] = "Opravdu smazat plán \"{0}\"?\nTato akce je nevratná.",
        ["WorkoutsCount"] = "{0} tréninků",
        ["PlansCount"] = "{0} plánů",
        ["ExercisesCount"] = "{0} cviků",
        ["Empty"] = "Prázdný",
        ["Draft"] = "koncept",

        // === MojeCestaPage ===
        ["YourStatsProgress"] = "Tvoje statistiky a progres",
        ["WorkoutsLower"] = "workouty",
        ["Lifted"] = "nazvedáno",
        ["Trained"] = "odcvičeno",
        ["RunWalked"] = "nabeháno / nachozeno",
        ["TotalBurned"] = "celkem spáleno",
        ["WorkoutsBySport"] = "TRÉNINKY PODLE SPORTU",
        ["YourStats"] = "Tvoje statistiky",
        ["StatsDesc"] = "Statistiky se budou automaticky počítat z tvých odcvičených workoutů. Začni cvičit a sleduj svůj progres!",
        ["NoWorkoutsYet"] = "Zatím žádné workouty — jdi cvičit!",
        ["Workout1"] = "trénink",
        ["Workout24"] = "tréninky",
        ["Workout5plus"] = "tréninků",

        // === ZacitWorkoutPage ===
        ["PickSportDesc"] = "Vyber si sport a jdeme na to",
        ["Cardio"] = "KARDIO",
        ["Strength"] = "SÍLA",
        ["Sports"] = "SPORTY",
        ["RelaxFlex"] = "RELAX & FLEXIBILITA",
        ["CalorieTracking"] = "Sledování kalorií",
        ["CalorieTrackingDesc"] = "Kalorie se počítají automaticky podle tvé váhy z profilu a doby tréninku. Nastav si váhu v Nastavení pro přesné výsledky.",

        // Sport names
        ["SportRun"] = "Běh",
        ["SportCycle"] = "Kolo",
        ["SportSwim"] = "Plavání",
        ["SportGym"] = "Posilovna",
        ["SportHiit"] = "HIIT",
        ["SportCrossfit"] = "CrossFit",
        ["SportFootball"] = "Fotbal",
        ["SportBasketball"] = "Basketbal",
        ["SportTennis"] = "Tenis",
        ["SportYoga"] = "Jóga",
        ["SportWalk"] = "Chůze",
        ["SportStretching"] = "Stretching",

        // === ChatPage ===
        ["AiGreeting"] = "Ahoj! Jsem tvůj AI trenér.",
        ["AiGreetingDesc"] = "Zeptej se mě na cokoliv o tréninku, výživě nebo regeneraci. (Powered by Gemini)",
        ["SuggestWorkout"] = "Navrhni trénink",
        ["HowToLoseWeight"] = "Jak zhubnout?",
        ["ExerciseTechnique"] = "Technika cviků",
        ["MotivateMe"] = "Motivuj mě",
        ["SuggestWorkoutFull"] = "Navrhni mi trénink na dnes",
        ["HowToLoseWeightFull"] = "Jak efektivně zhubnout?",
        ["ExerciseTechniqueFull"] = "Poraď mi s technikou cviků",
        ["MotivateMeFull"] = "Motivuj mě k tréninku!",
        ["AiThinking"] = "AI přemýšlí...",
        ["TypeMessage"] = "Napiš zprávu...",
        ["DeleteHistory"] = "Smazat historii",
        ["DeleteHistoryConfirm"] = "Opravdu chceš smazat celou historii chatu?",
        ["AskAnything"] = "Zeptej se mě na cokoliv",
        ["TrainingNutritionTech"] = "Trénink, výživa, technika cviků...",
        ["ConsultTraining"] = "Poraď se o tréninku",

        // === CviceniPage ===
        ["ExerciseDatabase"] = "Databáze cvičení",
        ["ExercisesTotal"] = "{0} cviků celkem",
        ["ExercisesInCategory"] = "{0} cviků v kategorii {1}",
        ["AllFilter"] = "🏅 Vše",
        ["NoExercises"] = "Žádné cviky",
        ["NewExercise"] = "Nový cvik",
        ["ExerciseName"] = "Název cviku",
        ["Add"] = "Přidat",
        ["ExerciseNamePlaceholder"] = "např. Bench press",
        ["SelectCategory"] = "Vyber kategorii",
        ["Description"] = "Popis",
        ["ShortDescOptional"] = "Krátký popis (volitelné)",
        ["Skip"] = "Přeskočit",

        // Categories
        ["CatChest"] = "Hrudník",
        ["CatBack"] = "Záda",
        ["CatLegs"] = "Nohy",
        ["CatShoulders"] = "Ramena",
        ["CatArms"] = "Ruce",
        ["CatCore"] = "Core",
        ["CatCardio"] = "Kardio",

        // === DetailTreninkuPage ===
        ["Exercise"] = "Cvik",
        ["Sets"] = "Série",
        ["RepsWeight"] = "Opakování/Váha",
        ["Rest"] = "Pauza",
        ["StartTraining"] = "ZAČÍT TRÉNINK",
        ["DeleteTraining"] = "Smazat trénink",
        ["DeleteTrainingConfirm"] = "Opravdu chceš smazat \"{0}\"?",

        // === SportTimerPage ===
        ["GetReady"] = "Připrav se...",
        ["TrainingTime"] = "čas tréninku",
        ["SoonWithWatch"] = "brzy s hodinkami",
        ["AreYouReady"] = "Jsi připravený?",
        ["StartWorkoutBtn"] = "ZAČÍT WORKOUT",
        ["EndWorkout"] = "UKONČIT WORKOUT",
        ["WorkoutInProgress"] = "Workout probíhá...",
        ["CalcInfo"] = "Výpočet: MET {0} × {1} kg × čas\nHodnota MET dle vědeckých standardů",
        ["GpsUnavailable"] = "\n📍 GPS nedostupná — vzdálenost se nebude měřit",
        ["GpsError"] = "\n📍 GPS chyba — vzdálenost se nebude měřit",
        ["WorkoutDone"] = "Workout dokončen!",
        ["SportLabel"] = "Sport: {0}",
        ["TimeLabel"] = "Čas: {0}",
        ["DistanceLabel"] = "Vzdálenost: {0} km",
        ["BurnedLabel"] = "Spáleno: {0} kcal",
        ["EndWorkoutQuestion"] = "Ukončit workout?",
        ["WorkoutNotSaved"] = "Workout nebude uložen do statistik.",
        ["EndBtn"] = "Ukončit",
        ["ContinueBtn"] = "Pokračovat",

        // === SpustitTreninkPage ===
        ["TrainingInProgress"] = "Trénink probíhá...",
        ["Time"] = "čas",
        ["Pause"] = "PAUZA",
        ["SkipPause"] = "PŘESKOČIT",
        ["EndTraining"] = "UKONČIT TRÉNINK",
        ["TrainingDone"] = "Trénink dokončen!",
        ["TimeDone"] = "Čas: {0}",
        ["LiftedDone"] = "Nazvedáno: {0} kg",
        ["BurnedDone"] = "Spáleno: {0} kcal",
        ["EndTrainingQuestion"] = "Ukončit trénink?",
        ["TrainingNotSaved"] = "Trénink nebude uložen do statistik.",

        // === NovyTreninkPage ===
        ["NewTraining"] = "Nový trénink",
        ["NameYourTraining"] = "Pojmenuj svůj trénink",
        ["TrainingNameExample"] = "např. Push day, Nohy, Full body...",
        ["TrainingNamePlaceholder"] = "Název tréninku",
        ["EnterTrainingName"] = "Zadej název tréninku",

        // === TreninkTabulkaPage ===
        ["SaveTraining"] = "Uložit trénink",
        ["ExerciseNameCol"] = "Cvik",
        ["SeriesCol"] = "Série {0}",
        ["RestCol"] = "Pauzy",
        ["NotesCol"] = "Poznámky",
        ["ExerciseNamePlaceholderCol"] = "Název cviku...",
        ["TrainingSaved"] = "Trénink \"{0}\" uložen s {1} cviky!",

        // === RegistracePage ===
        ["Registration"] = "Registrace",
        ["Register"] = "Zaregistrovat se",
        ["PhoneNumber"] = "Telefonní číslo",
        ["EmailSignIn"] = "Přihlášení e-mailem",
        ["EnterNameEmail"] = "Zadej jméno a e-mail",
        ["PhoneSignIn"] = "Přihlášení telefonem",
        ["EnterNamePhone"] = "Zadej jméno a telefonní číslo",
        ["AppleSignIn"] = "Přihlášení přes Apple",
        ["EnterNameApple"] = "Zadej jméno a přihlas se přes Apple ID",
        ["SignInAppleBtn"] = "Přihlásit přes Apple",
        ["AppleRedirectInfo"] = "Po kliknutí na tlačítko níže budeš přesměrován na přihlášení přes Apple ID. (Bude dostupné po napojení na server.)",
        ["EnterYourName"] = "Zadej své jméno",
        ["EnterValidEmail"] = "Zadej platný e-mail",
        ["EnterValidPhone"] = "Zadej platné telefonní číslo",
        ["Done"] = "Hotovo",
        ["WelcomeUser"] = "Vítej, {0}!",

        // === VyberCvikuPage ===
        ["SelectExercise"] = "Vyber cvik",
        ["UnknownExercise"] = "Neznámý cvik",

        // === Month abbreviations ===
        ["MonJan"] = "led",
        ["MonFeb"] = "úno",
        ["MonMar"] = "bře",
        ["MonApr"] = "dub",
        ["MonMay"] = "kvě",
        ["MonJun"] = "čvn",
        ["MonJul"] = "čvc",
        ["MonAug"] = "srp",
        ["MonSep"] = "zář",
        ["MonOct"] = "říj",
        ["MonNov"] = "lis",
        ["MonDec"] = "pro",

        // === AI System Prompt ===
        ["AiSystemRole"] = "Jsi osobní fitness trenér a poradce v aplikaci trAIn.",
        ["AiSystemTone"] = "Odpovídej vždy česky. Buď motivující, konkrétní a přátelský.",
        ["AiSystemBrief"] = "Drž odpovědi stručné a praktické.",
        ["AiUserProfile"] = "Profil uživatele:",
        ["AiProfileName"] = "Jméno",
        ["AiProfileAge"] = "Věk",
        ["AiProfileGender"] = "Pohlaví",
        ["AiProfileWeight"] = "Váha",
        ["AiProfileHeight"] = "Výška",
        ["AiProfileGoal"] = "Cíl",
        ["AiProfileLevel"] = "Úroveň",
        ["AiRecentWorkouts"] = "Poslední tréninky:",
        ["AiSystemAdvice"] = "Poskytuj personalizované rady o tréninku, technice, regeneraci a motivaci.",
        ["AiSystemNoMedical"] = "Nedávej lékařské rady. Pokud nemáš info o profilu, zeptej se.",

        // === API Error Messages ===
        ["ErrNoApiKey"] = "⚙️ Nemáš nastavený API klíč. Jdi do Nastavení a zadej svůj Gemini API klíč.",
        ["ErrInvalidKey"] = "❌ Neplatný API klíč. Zkontroluj ho v Nastavení.",
        ["ErrTooManyRequests"] = "⏳ Příliš mnoho požadavků. Počkej chvíli a zkus to znovu.",
        ["ErrApi"] = "❌ Chyba API ({0}). Zkus to znovu.",
        ["ErrEmptyResponse"] = "Prázdná odpověď od AI.",
        ["ErrTimeout"] = "⏱️ Vypršel časový limit. Zkontroluj připojení k internetu.",
        ["ErrConnection"] = "🌐 Chyba připojení. Zkontroluj internet a zkus to znovu.",
        ["ErrUnexpected"] = "❌ Neočekávaná chyba: {0}",
        ["ErrGeneric"] = "❌ Chyba: {0}",
    };

    // =========================================================
    //  ENGLISH TRANSLATIONS
    // =========================================================
    private static readonly Dictionary<string, string> _en = new()
    {
        // === Common ===
        ["OK"] = "OK",
        ["Cancel"] = "Cancel",
        ["Delete"] = "Delete",
        ["Save"] = "Save",
        ["Error"] = "Error",
        ["Saved"] = "Saved",
        ["Loading"] = "Loading...",
        ["Continue"] = "Continue",
        ["Back"] = "Back",

        // === MainPage ===
        ["MainSubtitle"] = "Your workouts in one place",
        ["MyJourney"] = "My journey",
        ["MyJourneyDesc"] = "Your stats and progress",
        ["MyWorkouts"] = "My workouts",
        ["MyWorkoutsDesc"] = "Create workouts and manage your plans",
        ["StartWorkout"] = "Start workout",
        ["StartWorkoutDesc"] = "Pick a sport and track performance",

        // === NastaveniPage ===
        ["Settings"] = "Settings",
        ["NoAccount"] = "No account",
        ["SignInToSync"] = "Sign in to sync your workouts",
        ["Email"] = "Email",
        ["RegisterOrSignIn"] = "Register or sign in",
        ["Phone"] = "Phone",
        ["SignInViaSms"] = "Sign in via SMS code",
        ["AppleId"] = "Apple ID",
        ["QuickSignInApple"] = "Quick sign in with Apple",
        ["SignOut"] = "Sign out",
        ["SignOutConfirm"] = "Do you really want to sign out?",
        ["SignedVia"] = "Signed in via {0}",
        ["Exercises"] = "Exercises",
        ["ExerciseLibrary"] = "Exercise library",
        ["ExerciseLibraryDesc"] = "Browse and edit exercise database",
        ["AiTrainer"] = "AI Trainer",
        ["GeminiApiKey"] = "Gemini API key (free)",
        ["SaveApiKey"] = "Save API key",
        ["KeyNotSet"] = "Key not set",
        ["KeySet"] = "✅ Key set ({0})",
        ["EnterApiKey"] = "Enter API key.",
        ["ApiKeySaved"] = "API key has been saved.",
        ["Appearance"] = "Appearance",
        ["DarkMode"] = "Dark mode",
        ["LightMode"] = "Light mode",
        ["Active"] = "Active",
        ["Profile"] = "Profile",
        ["Name"] = "Name",
        ["YourName"] = "Your name",
        ["Age"] = "Age",
        ["Gender"] = "Gender",
        ["Male"] = "Male",
        ["Female"] = "Female",
        ["WeightKg"] = "Weight (kg)",
        ["HeightCm"] = "Height (cm)",
        ["YourGoal"] = "Your goal",
        ["BuildMuscle"] = "Build muscle",
        ["LoseWeight"] = "Lose weight",
        ["Maintain"] = "Maintain",
        ["Fitness"] = "Fitness",
        ["Level"] = "Level",
        ["Beginner"] = "Beginner",
        ["Intermediate"] = "Intermediate",
        ["Expert"] = "Expert",
        ["Underweight"] = "Underweight",
        ["Normal"] = "Normal",
        ["Overweight"] = "Overweight",
        ["Obese"] = "Obese",
        ["SaveProfile"] = "Save profile",
        ["ProfileSaved"] = "Profile has been saved.",
        ["Language"] = "Language",
        ["Czech"] = "Čeština",
        ["English"] = "English",
        ["LanguageDesc"] = "App language",

        // === TreninkyPage ===
        ["Workouts"] = "Workouts",
        ["Plans"] = "Plans",
        ["CreateNew"] = "+ Create",
        ["CreateNewPlan"] = "Create new plan",
        ["NamePlanDesc"] = "Name your plan, pick a date and let's go",
        ["PlanNamePlaceholder"] = "Plan name (e.g. Push day, Legs...)",
        ["CreatePlan"] = "Create plan",
        ["NoWorkouts"] = "No workouts",
        ["WorkoutsFromPlans"] = "Workouts are created automatically from plans",
        ["NoPlans"] = "No plans",
        ["CreateFirstPlan"] = "Create your first training plan",
        ["NewPlan"] = "New plan",
        ["EnterPlanName"] = "Enter plan name",
        ["DeletePlan"] = "Delete plan",
        ["DeletePlanConfirm"] = "Really delete plan \"{0}\"?\nThis action is irreversible.",
        ["WorkoutsCount"] = "{0} workouts",
        ["PlansCount"] = "{0} plans",
        ["ExercisesCount"] = "{0} exercises",
        ["Empty"] = "Empty",
        ["Draft"] = "draft",

        // === MojeCestaPage ===
        ["YourStatsProgress"] = "Your stats and progress",
        ["WorkoutsLower"] = "workouts",
        ["Lifted"] = "lifted",
        ["Trained"] = "trained",
        ["RunWalked"] = "run / walked",
        ["TotalBurned"] = "total burned",
        ["WorkoutsBySport"] = "WORKOUTS BY SPORT",
        ["YourStats"] = "Your statistics",
        ["StatsDesc"] = "Statistics are calculated automatically from your completed workouts. Start exercising and track your progress!",
        ["NoWorkoutsYet"] = "No workouts yet — go exercise!",
        ["Workout1"] = "workout",
        ["Workout24"] = "workouts",
        ["Workout5plus"] = "workouts",

        // === ZacitWorkoutPage ===
        ["PickSportDesc"] = "Pick a sport and let's go",
        ["Cardio"] = "CARDIO",
        ["Strength"] = "STRENGTH",
        ["Sports"] = "SPORTS",
        ["RelaxFlex"] = "RELAX & FLEXIBILITY",
        ["CalorieTracking"] = "Calorie tracking",
        ["CalorieTrackingDesc"] = "Calories are calculated automatically based on your weight from profile and workout duration. Set your weight in Settings for accurate results.",

        // Sport names
        ["SportRun"] = "Running",
        ["SportCycle"] = "Cycling",
        ["SportSwim"] = "Swimming",
        ["SportGym"] = "Gym",
        ["SportHiit"] = "HIIT",
        ["SportCrossfit"] = "CrossFit",
        ["SportFootball"] = "Football",
        ["SportBasketball"] = "Basketball",
        ["SportTennis"] = "Tennis",
        ["SportYoga"] = "Yoga",
        ["SportWalk"] = "Walking",
        ["SportStretching"] = "Stretching",

        // === ChatPage ===
        ["AiGreeting"] = "Hi! I'm your AI trainer.",
        ["AiGreetingDesc"] = "Ask me anything about training, nutrition or recovery. (Powered by Gemini)",
        ["SuggestWorkout"] = "Suggest workout",
        ["HowToLoseWeight"] = "How to lose weight?",
        ["ExerciseTechnique"] = "Exercise technique",
        ["MotivateMe"] = "Motivate me",
        ["SuggestWorkoutFull"] = "Suggest a workout for today",
        ["HowToLoseWeightFull"] = "How to lose weight effectively?",
        ["ExerciseTechniqueFull"] = "Give me exercise technique tips",
        ["MotivateMeFull"] = "Motivate me to work out!",
        ["AiThinking"] = "AI is thinking...",
        ["TypeMessage"] = "Type a message...",
        ["DeleteHistory"] = "Delete history",
        ["DeleteHistoryConfirm"] = "Do you really want to delete the entire chat history?",
        ["AskAnything"] = "Ask me anything",
        ["TrainingNutritionTech"] = "Training, nutrition, exercise technique...",
        ["ConsultTraining"] = "Get training advice",

        // === CviceniPage ===
        ["ExerciseDatabase"] = "Exercise database",
        ["ExercisesTotal"] = "{0} exercises total",
        ["ExercisesInCategory"] = "{0} exercises in {1}",
        ["AllFilter"] = "🏅 All",
        ["NoExercises"] = "No exercises",
        ["NewExercise"] = "New exercise",
        ["ExerciseName"] = "Exercise name",
        ["Add"] = "Add",
        ["ExerciseNamePlaceholder"] = "e.g. Bench press",
        ["SelectCategory"] = "Select category",
        ["Description"] = "Description",
        ["ShortDescOptional"] = "Short description (optional)",
        ["Skip"] = "Skip",

        // Categories
        ["CatChest"] = "Chest",
        ["CatBack"] = "Back",
        ["CatLegs"] = "Legs",
        ["CatShoulders"] = "Shoulders",
        ["CatArms"] = "Arms",
        ["CatCore"] = "Core",
        ["CatCardio"] = "Cardio",

        // === DetailTreninkuPage ===
        ["Exercise"] = "Exercise",
        ["Sets"] = "Sets",
        ["RepsWeight"] = "Reps/Weight",
        ["Rest"] = "Rest",
        ["StartTraining"] = "START TRAINING",
        ["DeleteTraining"] = "Delete training",
        ["DeleteTrainingConfirm"] = "Do you really want to delete \"{0}\"?",

        // === SportTimerPage ===
        ["GetReady"] = "Get ready...",
        ["TrainingTime"] = "training time",
        ["SoonWithWatch"] = "coming soon with watch",
        ["AreYouReady"] = "Are you ready?",
        ["StartWorkoutBtn"] = "START WORKOUT",
        ["EndWorkout"] = "END WORKOUT",
        ["WorkoutInProgress"] = "Workout in progress...",
        ["CalcInfo"] = "Calculation: MET {0} × {1} kg × time\nMET value based on scientific standards",
        ["GpsUnavailable"] = "\n📍 GPS unavailable — distance won't be tracked",
        ["GpsError"] = "\n📍 GPS error — distance won't be tracked",
        ["WorkoutDone"] = "Workout complete!",
        ["SportLabel"] = "Sport: {0}",
        ["TimeLabel"] = "Time: {0}",
        ["DistanceLabel"] = "Distance: {0} km",
        ["BurnedLabel"] = "Burned: {0} kcal",
        ["EndWorkoutQuestion"] = "End workout?",
        ["WorkoutNotSaved"] = "Workout won't be saved to statistics.",
        ["EndBtn"] = "End",
        ["ContinueBtn"] = "Continue",

        // === SpustitTreninkPage ===
        ["TrainingInProgress"] = "Training in progress...",
        ["Time"] = "time",
        ["Pause"] = "PAUSE",
        ["SkipPause"] = "SKIP",
        ["EndTraining"] = "END TRAINING",
        ["TrainingDone"] = "Training complete!",
        ["TimeDone"] = "Time: {0}",
        ["LiftedDone"] = "Lifted: {0} kg",
        ["BurnedDone"] = "Burned: {0} kcal",
        ["EndTrainingQuestion"] = "End training?",
        ["TrainingNotSaved"] = "Training won't be saved to statistics.",

        // === NovyTreninkPage ===
        ["NewTraining"] = "New training",
        ["NameYourTraining"] = "Name your training",
        ["TrainingNameExample"] = "e.g. Push day, Legs, Full body...",
        ["TrainingNamePlaceholder"] = "Training name",
        ["EnterTrainingName"] = "Enter training name",

        // === TreninkTabulkaPage ===
        ["SaveTraining"] = "Save training",
        ["ExerciseNameCol"] = "Exercise",
        ["SeriesCol"] = "Set {0}",
        ["RestCol"] = "Rest",
        ["NotesCol"] = "Notes",
        ["ExerciseNamePlaceholderCol"] = "Exercise name...",
        ["TrainingSaved"] = "Training \"{0}\" saved with {1} exercises!",

        // === RegistracePage ===
        ["Registration"] = "Registration",
        ["Register"] = "Register",
        ["PhoneNumber"] = "Phone number",
        ["EmailSignIn"] = "Sign in with email",
        ["EnterNameEmail"] = "Enter name and email",
        ["PhoneSignIn"] = "Sign in with phone",
        ["EnterNamePhone"] = "Enter name and phone number",
        ["AppleSignIn"] = "Sign in with Apple",
        ["EnterNameApple"] = "Enter name and sign in with Apple ID",
        ["SignInAppleBtn"] = "Sign in with Apple",
        ["AppleRedirectInfo"] = "After clicking the button below, you will be redirected to Apple ID sign in. (Will be available after server integration.)",
        ["EnterYourName"] = "Enter your name",
        ["EnterValidEmail"] = "Enter a valid email",
        ["EnterValidPhone"] = "Enter a valid phone number",
        ["Done"] = "Done",
        ["WelcomeUser"] = "Welcome, {0}!",

        // === VyberCvikuPage ===
        ["SelectExercise"] = "Select exercise",
        ["UnknownExercise"] = "Unknown exercise",

        // === Month abbreviations ===
        ["MonJan"] = "Jan",
        ["MonFeb"] = "Feb",
        ["MonMar"] = "Mar",
        ["MonApr"] = "Apr",
        ["MonMay"] = "May",
        ["MonJun"] = "Jun",
        ["MonJul"] = "Jul",
        ["MonAug"] = "Aug",
        ["MonSep"] = "Sep",
        ["MonOct"] = "Oct",
        ["MonNov"] = "Nov",
        ["MonDec"] = "Dec",

        // === AI System Prompt ===
        ["AiSystemRole"] = "You are a personal fitness trainer and advisor in the trAIn app.",
        ["AiSystemTone"] = "Always reply in English. Be motivating, specific and friendly.",
        ["AiSystemBrief"] = "Keep your answers concise and practical.",
        ["AiUserProfile"] = "User profile:",
        ["AiProfileName"] = "Name",
        ["AiProfileAge"] = "Age",
        ["AiProfileGender"] = "Gender",
        ["AiProfileWeight"] = "Weight",
        ["AiProfileHeight"] = "Height",
        ["AiProfileGoal"] = "Goal",
        ["AiProfileLevel"] = "Level",
        ["AiRecentWorkouts"] = "Recent workouts:",
        ["AiSystemAdvice"] = "Provide personalized advice on training, technique, recovery and motivation.",
        ["AiSystemNoMedical"] = "Do not give medical advice. If you don't have profile info, ask.",

        // === API Error Messages ===
        ["ErrNoApiKey"] = "⚙️ API key not set. Go to Settings and enter your Gemini API key.",
        ["ErrInvalidKey"] = "❌ Invalid API key. Check it in Settings.",
        ["ErrTooManyRequests"] = "⏳ Too many requests. Wait a moment and try again.",
        ["ErrApi"] = "❌ API error ({0}). Try again.",
        ["ErrEmptyResponse"] = "Empty response from AI.",
        ["ErrTimeout"] = "⏱️ Request timed out. Check your internet connection.",
        ["ErrConnection"] = "🌐 Connection error. Check your internet and try again.",
        ["ErrUnexpected"] = "❌ Unexpected error: {0}",
        ["ErrGeneric"] = "❌ Error: {0}",
    };

    /// <summary>
    /// Gets the translated sport name by sport key.
    /// </summary>
    public static string SportName(string sportKey) => sportKey?.ToLowerInvariant() switch
    {
        "beh" => T("SportRun"),
        "kolo" => T("SportCycle"),
        "plavani" => T("SportSwim"),
        "posilovna" => T("SportGym"),
        "hiit" => T("SportHiit"),
        "crossfit" => T("SportCrossfit"),
        "fotbal" => T("SportFootball"),
        "basketbal" => T("SportBasketball"),
        "tenis" => T("SportTennis"),
        "joga" => T("SportYoga"),
        "chuze" => T("SportWalk"),
        "stretching" => T("SportStretching"),
        _ => sportKey ?? ""
    };

    /// <summary>
    /// Gets month abbreviation (1-based index).
    /// </summary>
    public static string MonthAbbr(int month) => month switch
    {
        1 => T("MonJan"), 2 => T("MonFeb"), 3 => T("MonMar"),
        4 => T("MonApr"), 5 => T("MonMay"), 6 => T("MonJun"),
        7 => T("MonJul"), 8 => T("MonAug"), 9 => T("MonSep"),
        10 => T("MonOct"), 11 => T("MonNov"), 12 => T("MonDec"),
        _ => ""
    };

    /// <summary>
    /// Gets the localized category name.
    /// </summary>
    public static string CategoryName(string category) => category switch
    {
        "Hrudník" => T("CatChest"),
        "Záda" => T("CatBack"),
        "Nohy" => T("CatLegs"),
        "Ramena" => T("CatShoulders"),
        "Ruce" => T("CatArms"),
        "Core" => T("CatCore"),
        "Kardio" => T("CatCardio"),
        _ => category
    };

    /// <summary>
    /// Workout count with proper Czech/English pluralization.
    /// </summary>
    public static string WorkoutCount(int count) => count switch
    {
        1 => T("Workout1"),
        >= 2 and <= 4 => T("Workout24"),
        _ => T("Workout5plus")
    };
}
