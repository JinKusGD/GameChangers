using System.Collections.Generic;

public class SkillRunner
{
    private readonly List<Skill> _skills = new List<Skill>();

    public void Register(Skill skill)
    {
        if (_skills.Contains(skill)) { return; }

        _skills.Add(skill);
    }

    public void Unregister(Skill skill)
    {
        _skills.Remove(skill);
    }

    public void Tick(float deltaTime)
    {
        for (int i = _skills.Count - 1; i >= 0; i--)
        {
            _skills[i].Tick(deltaTime);
        }
    }
}
