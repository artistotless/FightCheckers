using UnityEngine;

public class Fight
{
    private FightSettings _settings;
    private Transform _enemy;

    public Fight (Transform enemy, FightSettings settings)
    {
        _settings = settings;
        _enemy = enemy;
    }
}
