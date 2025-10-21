using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages player contracts, salaries, and team budgets
/// </summary>
public class ContractSystem : MonoBehaviour
{
    [System.Serializable]
    public class PlayerContract
    {
        public CSPlayer player;
        public Team team;
        public float monthlySalary;
        public DateTime contractStart;
        public DateTime contractEnd;
        public float signingBonus;
        public float performanceBonus; // Additional pay based on performance
        public ContractStatus status;
        public List<ContractClause> clauses;

        public bool IsActive()
        {
            return DateTime.Now >= contractStart && DateTime.Now <= contractEnd;
        }

        public float GetRemainingMonths()
        {
            return (float)(contractEnd - DateTime.Now).TotalDays / 30f;
        }

        public bool IsExpiringSoon()
        {
            return GetRemainingMonths() < 1;
        }
    }

    [System.Serializable]
    public class ContractClause
    {
        public string clauseName;
        public float clauseValue;
        public ClauseType clauseType; // Buyout, Release, Championship Bonus, etc.
    }

    [System.Serializable]
    public class TeamBudget
    {
        public Team team;
        public float totalBudget;
        public float spentOnSalaries;
        public float spentOnBonuses;
        public float sponsorshipIncome;
        public float prizeMoneyEarned;
        public List<BudgetTransaction> transactions;

        public float GetAvailableBudget()
        {
            return totalBudget + sponsorshipIncome + prizeMoneyEarned - spentOnSalaries - spentOnBonuses;
        }
    }

    [System.Serializable]
    public class BudgetTransaction
    {
        public string description;
        public float amount;
        public DateTime date;
        public TransactionType type;
    }

    public enum ContractStatus { Active, Pending, Expired, Terminated, OnLoan }
    public enum ClauseType { Buyout, ReleaseClause, ChampionshipBonus, PerformanceBonus, LoyaltyBonus }
    public enum TransactionType { Salary, Bonus, Income, Expense, PrizeMoney }

    private Dictionary<Team, TeamBudget> teamBudgets = new();
    private Dictionary<CSPlayer, PlayerContract> playerContracts = new();

    private float monthlyExpenses = 0f;

    public void InitializeBudget(Team team, float initialBudget)
    {
        if (!teamBudgets.ContainsKey(team))
        {
            teamBudgets[team] = new TeamBudget
            {
                team = team,
                totalBudget = initialBudget,
                spentOnSalaries = 0f,
                spentOnBonuses = 0f,
                sponsorshipIncome = 0f,
                prizeMoneyEarned = 0f,
                transactions = new List<BudgetTransaction>()
            };
        }
    }

    public bool SignContract(CSPlayer player, Team team, float monthlySalary,
        float signingBonus, int contractMonths)
    {
        TeamBudget budget = teamBudgets[team];

        // Check if team can afford the contract
        float totalCost = (monthlySalary * contractMonths) + signingBonus;
        if (budget.GetAvailableBudget() < totalCost)
        {
            Debug.LogWarning($"Team {team.teamName} cannot afford contract for {player.playerName}");
            return false;
        }

        // Check if player is already contracted elsewhere
        if (playerContracts.ContainsKey(player) && playerContracts[player].IsActive())
        {
            Debug.LogWarning($"Player {player.playerName} is already under contract");
            return false;
        }

        PlayerContract contract = new()
        {
            player = player,
            team = team,
            monthlySalary = monthlySalary,
            signingBonus = signingBonus,
            contractStart = DateTime.Now,
            contractEnd = DateTime.Now.AddMonths(contractMonths),
            status = ContractStatus.Active,
            clauses = new List<ContractClause>()
        };

        playerContracts[player] = contract;

        // Deduct signing bonus immediately
        budget.spentOnBonuses += signingBonus;
        budget.spentOnSalaries += monthlySalary * contractMonths;

        AddTransaction(team, $"Signed {player.playerName}", signingBonus, TransactionType.Expense);

        Debug.Log($"Contract signed: {player.playerName} with {team.teamName}");
        return true;
    }

    public void AddContractClause(CSPlayer player, ContractClause clause)
    {
        if (playerContracts.ContainsKey(player))
        {
            playerContracts[player].clauses.Add(clause);
        }
    }

    public void ProcessMonthlySalaries()
    {
        foreach (var kvp in playerContracts)
        {
            if (kvp.Value.IsActive())
            {
                CSPlayer player = kvp.Key;
                PlayerContract contract = kvp.Value;
                Team team = contract.team;

                if (teamBudgets.ContainsKey(team))
                {
                    TeamBudget budget = teamBudgets[team];
                    float salary = contract.monthlySalary;

                    budget.spentOnSalaries += salary;
                    AddTransaction(team, $"Salary payment to {player.playerName}", salary,
                        TransactionType.Salary);
                }
            }
        }
    }

    public void TerminateContract(CSPlayer player)
    {
        if (!playerContracts.ContainsKey(player))
            return;

        PlayerContract contract = playerContracts[player];
        TeamBudget budget = teamBudgets[contract.team];

        // Check if contract has buyout clause
        ContractClause buyoutClause = contract.clauses.Find(c => c.clauseType == ClauseType.Buyout);
        float terminationCost = buyoutClause != null ? buyoutClause.clauseValue : contract.monthlySalary * 3;

        if (budget.GetAvailableBudget() < terminationCost)
        {
            Debug.LogWarning($"Team cannot afford to terminate {player.playerName}'s contract");
            return;
        }

        budget.spentOnBonuses += terminationCost;
        contract.status = ContractStatus.Terminated;
        playerContracts.Remove(player);

        AddTransaction(contract.team, $"Contract termination for {player.playerName}",
            terminationCost, TransactionType.Expense);
    }

    public void AddPrizeMoneyIncome(Team team, float amount)
    {
        if (!teamBudgets.ContainsKey(team))
            return;

        TeamBudget budget = teamBudgets[team];
        budget.prizeMoneyEarned += amount;
        AddTransaction(team, "Prize money earned", amount, TransactionType.PrizeMoney);
    }

    public void AddSponsorshipIncome(Team team, float amount)
    {
        if (!teamBudgets.ContainsKey(team))
            return;

        TeamBudget budget = teamBudgets[team];
        budget.sponsorshipIncome += amount;
        AddTransaction(team, "Sponsorship income", amount, TransactionType.Income);
    }

    public float GetTeamAvailableBudget(Team team)
    {
        return teamBudgets.ContainsKey(team) ? teamBudgets[team].GetAvailableBudget() : 0f;
    }

    public PlayerContract GetPlayerContract(CSPlayer player)
    {
        return playerContracts.ContainsKey(player) ? playerContracts[player] : null;
    }

    public TeamBudget GetTeamBudget(Team team)
    {
        return teamBudgets.ContainsKey(team) ? teamBudgets[team] : null;
    }

    public List<PlayerContract> GetTeamContracts(Team team)
    {
        List<PlayerContract> contracts = new();
        foreach (var kvp in playerContracts)
        {
            if (kvp.Value.team == team && kvp.Value.IsActive())
            {
                contracts.Add(kvp.Value);
            }
        }
        return contracts;
    }

    private void AddTransaction(Team team, string description, float amount, TransactionType type)
    {
        if (teamBudgets.ContainsKey(team))
        {
            BudgetTransaction transaction = new()
            {
                description = description,
                amount = amount,
                date = DateTime.Now,
                type = type
            };
            teamBudgets[team].transactions.Add(transaction);
        }
    }
}