using System;
using System.Collections.Generic;
using UnityEngine;
using static ContractSystem;
using Object = UnityEngine.Object;

/// <summary>
/// Handles player trading and transfer system
/// </summary>
public class TransferMarket : MonoBehaviour
{
    [System.Serializable]
    public class TransferListing
    {
        public CSPlayer player;
        public Team currentTeam;
        public float askingPrice;
        public TransferStatus status;
        public DateTime listingDate;
        public DateTime deadline;
        public string description;
        public List<TransferOffer> offers;

        public bool IsActive()
        {
            return DateTime.Now < deadline && status == TransferStatus.Active;
        }

        public int GetDaysRemaining()
        {
            return (int)(deadline - DateTime.Now).TotalDays;
        }
    }

    [System.Serializable]
    public class TransferOffer
    {
        public Team biddingTeam;
        public float offerAmount;
        public DateTime offerDate;
        public OfferStatus status;
        public int contractLengthMonths;
        public float proposedMonthlySalary;
    }

    public enum TransferStatus { Active, Accepted, Rejected, Expired, Pending }
    public enum OfferStatus { Pending, Accepted, Rejected, Withdrawn }

    private Dictionary<CSPlayer, TransferListing> activeListings = new();
    private List<TransferListing> transferHistory = new();
    private ContractSystem contractSystem;

    private void Start()
    {
        contractSystem = GetComponent<ContractSystem>();
        if (contractSystem == null)
        {
            contractSystem = Object.FindFirstObjectByType<ContractSystem>();
        }
    }

    public void ListPlayerForTransfer(CSPlayer player, Team currentTeam, float askingPrice,
        int listingDaysRemaining = 30)
    {
        if (activeListings.ContainsKey(player))
        {
            Debug.LogWarning($"Player {player.playerName} is already listed for transfer");
            return;
        }

        TransferListing listing = new()
        {
            player = player,
            currentTeam = currentTeam,
            askingPrice = askingPrice,
            status = TransferStatus.Active,
            listingDate = DateTime.Now,
            deadline = DateTime.Now.AddDays(listingDaysRemaining),
            offers = new List<TransferOffer>(),
            description = $"{player.playerName} - Listed by {currentTeam.teamName}"
        };

        activeListings[player] = listing;
        Debug.Log($"Player {player.playerName} listed for transfer - Asking price: ${askingPrice}");
    }

    public void RemovePlayerFromTransfer(CSPlayer player)
    {
        if (activeListings.ContainsKey(player))
        {
            activeListings[player].status = TransferStatus.Expired;
            activeListings.Remove(player);
            Debug.Log($"Player {player.playerName} removed from transfer market");
        }
    }

    public bool PlaceTransferOffer(CSPlayer player, Team biddingTeam, float offerAmount,
        int contractLengthMonths, float proposedMonthlySalary)
    {
        if (!activeListings.ContainsKey(player))
        {
            Debug.LogWarning($"Player {player.playerName} is not listed for transfer");
            return false;
        }

        TransferListing listing = activeListings[player];

        if (!listing.IsActive())
        {
            Debug.LogWarning($"Transfer listing for {player.playerName} is no longer active");
            return false;
        }

        // Check if team has budget for this transfer
        float totalCost = offerAmount + (proposedMonthlySalary * contractLengthMonths);
        if (contractSystem.GetTeamAvailableBudget(biddingTeam) < totalCost)
        {
            Debug.LogWarning($"Team {biddingTeam.teamName} does not have sufficient budget");
            return false;
        }

        TransferOffer offer = new()
        {
            biddingTeam = biddingTeam,
            offerAmount = offerAmount,
            offerDate = DateTime.Now,
            status = OfferStatus.Pending,
            contractLengthMonths = contractLengthMonths,
            proposedMonthlySalary = proposedMonthlySalary
        };

        listing.offers.Add(offer);
        Debug.Log($"Transfer offer placed: {biddingTeam.teamName} offers ${offerAmount} for {player.playerName}");

        return true;
    }

    public bool AcceptTransferOffer(CSPlayer player, TransferOffer offer)
    {
        if (!activeListings.ContainsKey(player))
        {
            Debug.LogWarning($"Player {player.playerName} is not listed for transfer");
            return false;
        }

        TransferListing listing = activeListings[player];
        Team newTeam = offer.biddingTeam;
        Team oldTeam = listing.currentTeam;

        // Process the transfer
        if (contractSystem != null)
        {
            // Terminate old contract if exists
            PlayerContract oldContract = contractSystem.GetPlayerContract(player);
            if (oldContract != null)
            {
                contractSystem.TerminateContract(player);
            }

            // Sign new contract
            bool contractSigned = contractSystem.SignContract(player, newTeam,
                offer.proposedMonthlySalary, offer.offerAmount, offer.contractLengthMonths);

            if (!contractSigned)
            {
                Debug.LogError($"Failed to sign contract during transfer");
                return false;
            }
        }

        // Update transfer listing
        listing.status = TransferStatus.Accepted;
        offer.status = OfferStatus.Accepted;

        // Move player to new team
        oldTeam.roster.Remove(player);
        newTeam.roster.Add(player);

        // Archive transfer
        transferHistory.Add(listing);
        activeListings.Remove(player);

        Debug.Log($"Transfer complete: {player.playerName} transferred from {oldTeam.teamName} to {newTeam.teamName} for ${offer.offerAmount}");

        return true;
    }

    public void RejectTransferOffer(CSPlayer player, TransferOffer offer)
    {
        offer.status = OfferStatus.Rejected;
        Debug.Log($"Transfer offer rejected for {player.playerName}");
    }

    public List<TransferListing> GetActiveListings()
    {
        return new List<TransferListing>(activeListings.Values);
    }

    public TransferListing GetPlayerListing(CSPlayer player)
    {
        return activeListings.ContainsKey(player) ? activeListings[player] : null;
    }

    public List<TransferListing> GetTeamTransfers(Team team)
    {
        List<TransferListing> teamTransfers = new();
        foreach (var kvp in activeListings)
        {
            if (kvp.Value.currentTeam == team)
            {
                teamTransfers.Add(kvp.Value);
            }
        }
        return teamTransfers;
    }

    public void CheckExpiredListings()
    {
        List<CSPlayer> expiredPlayers = new();

        foreach (var kvp in activeListings)
        {
            if (!kvp.Value.IsActive())
            {
                expiredPlayers.Add(kvp.Key);
                kvp.Value.status = TransferStatus.Expired;
            }
        }

        foreach (var player in expiredPlayers)
        {
            activeListings.Remove(player);
            Debug.Log($"Transfer listing expired for {player.playerName}");
        }
    }

    public float CalculatePlayerValue(CSPlayer player, Team currentTeam = null)
    {
        // Base value calculation based on player attributes
        float baseSkill = (player.aim + player.gameIntelligence + player.consistency +
                          player.reflexes + player.utilityUsage) / 5f;

        float baseValue = baseSkill * 50000f; // Scale to reasonable transfer fee

        // Adjust for experience/age
        float ageAdjustment = 1.0f - (Mathf.Abs(player.age - 25) * 0.02f);
        baseValue *= ageAdjustment;

        // Adjust for team prestige if applicable
        if (currentTeam != null)
        {
            baseValue *= 1.1f; // Established players cost more
        }

        return Mathf.Max(10000f, baseValue); // Minimum value
    }
}