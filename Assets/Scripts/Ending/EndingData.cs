/// <summary>
/// Dane przekazywane do EndingScene (ładowanej addytywnie), wzorzec jak DaySummaryData.
/// DayEndHandler wypełnia je tuż przed wczytaniem sceny zakończenia, a
/// EndingSceneManager odczytuje je w Start().
/// </summary>
public static class EndingData
{
    /// a) prowadzący przeżywa (true) / b) ginie (false)
    public static bool HostSurvives;
    /// a) subkultury jednoczą się (true) / b) walki (false)
    public static bool ListenersUnite;
    /// a) sygnał ewakuacyjny (true) / b) cisza (false)
    public static bool GovernmentSignal;
    /// Końcowa liczba słuchaczy pokazywana na liczniku.
    public static int FinalListeners;
}
