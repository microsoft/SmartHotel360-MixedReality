using UnityEngine;

public class DoubleClickPreventer
{
    private readonly float _clickTimeOut;

    private float _lastClick = 0;

    public DoubleClickPreventer(float clickTimeOut = 0.1f)
    {
        _clickTimeOut = clickTimeOut;
    }

    public bool CanClick()
    {
        if (!(UnityEngine.Time.time - _lastClick > _clickTimeOut))
        {
            return false;
        }
        _lastClick = Time.time;
        return true;
    }
}
