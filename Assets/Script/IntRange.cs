using System;

[Serializable]
public class IntRange
{
    public int m_Min;//min value
    public int m_Max;//max value

    //constructor
   public  IntRange(int min, int max)
    {
        m_Min = min;
        m_Max = max;
    }

    //get random value from the range
    public int Random()
    {
        return UnityEngine.Random.Range(m_Min, m_Max);
    }
}