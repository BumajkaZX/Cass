namespace Cass.FirstEntry
{
    public class FirstEntry 
    {
        public bool IsFirstEntry()
        {
            AbstractFirstEntry firstEntry = default;

#if UNITY_EDITOR 

            firstEntry = new DummyFirstEntry();

#elif UNITY_ANDROID

            firstEntry = new AndroidFirstEntry();

#endif
            return firstEntry.IsFirstEntry();

        }
    }
}
