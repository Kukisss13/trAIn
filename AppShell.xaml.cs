using TreninkovyPlanovac.Views;

namespace TreninkovyPlanovac;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute("cviceni", typeof(CviceniPage));
		Routing.RegisterRoute("novytrenink", typeof(NovyTreninkPage));
		Routing.RegisterRoute("trenink-tabulka", typeof(TreninkTabulkaPage));
		Routing.RegisterRoute("vybercviku", typeof(VyberCvikuPage));
		Routing.RegisterRoute("treninky", typeof(TreninkyPage));
		Routing.RegisterRoute("detailtreninku", typeof(DetailTreninkuPage));
		Routing.RegisterRoute("nastaveni", typeof(NastaveniPage));
		Routing.RegisterRoute("registrace", typeof(RegistracePage));
		Routing.RegisterRoute("zacitworkout", typeof(ZacitWorkoutPage));
		Routing.RegisterRoute("mojecesta", typeof(MojeCestaPage));
		Routing.RegisterRoute("spustittrenink", typeof(SpustitTreninkPage));
		Routing.RegisterRoute("sporttimer", typeof(SportTimerPage));
		Routing.RegisterRoute("chat", typeof(ChatPage));
	}
}
