using System.Collections.Generic;
using UnityEngine;

public class Country
{
    public string name;
    public string code;
    public Sprite flag;
    public List<TournamentOrganizer> tournaments = new();
}