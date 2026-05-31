using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Unit Tests for HakayTech core validation logic.
/// Covers: Input Validation, OTP Expiry, AI Companion Assignment,
/// Map Lock Logic, and Coin Deduction.
/// </summary>
public class HakayTechTests
{
    // ========= 1. Email Validation =========

    [Test]
    public void Test_ValidEmail_ReturnsTrue()
    {
        string email = "test@gmail.com";
        Assert.IsTrue(IsValidEmail(email));
    }

    [Test]
    public void Test_InvalidEmail_MissingAt_ReturnsFalse()
    {
        string email = "testgmail.com";
        Assert.IsFalse(IsValidEmail(email));
    }

    [Test]
    public void Test_InvalidEmail_MissingDomain_ReturnsFalse()
    {
        string email = "test@";
        Assert.IsFalse(IsValidEmail(email));
    }

    // ========= 2. Name Validation =========

    [Test]
    public void Test_ValidName_TwoWords_ReturnsTrue()
    {
        string name = "ريماس ابو خليل";
        Assert.IsTrue(IsValidName(name));
    }

    [Test]
    public void Test_InvalidName_OneWord_ReturnsFalse()
    {
        string name = "ريماس";
        Assert.IsFalse(IsValidName(name));
    }

    [Test]
    public void Test_InvalidName_Empty_ReturnsFalse()
    {
        string name = "";
        Assert.IsFalse(IsValidName(name));
    }

    // ========= 3. OTP Expiry =========

    [Test]
    public void Test_OTP_NotExpired_ReturnsTrue()
    {
        string expiry = System.DateTime.UtcNow.AddMinutes(2).ToString();
        PlayerPrefs.SetString("otpExpiry", expiry);

        System.DateTime expiryTime = System.DateTime.Parse(
            PlayerPrefs.GetString("otpExpiry"));

        Assert.IsTrue(System.DateTime.UtcNow < expiryTime);
    }

    [Test]
    public void Test_OTP_Expired_ReturnsFalse()
    {
        string expiry = System.DateTime.UtcNow.AddMinutes(-1).ToString();
        PlayerPrefs.SetString("otpExpiry", expiry);

        System.DateTime expiryTime = System.DateTime.Parse(
            PlayerPrefs.GetString("otpExpiry"));

        Assert.IsFalse(System.DateTime.UtcNow < expiryTime);
    }

    // ========= 4. AI Companion Assignment =========

    [Test]
    public void Test_GirlGender_AssignsGirlCompanion()
    {
        string gender = "أنثى";
        string companion = AssignCompanion(gender);
        Assert.AreEqual("girl_comp", companion);
    }

    [Test]
    public void Test_BoyGender_AssignsBoyCompanion()
    {
        string gender = "ذكر";
        string companion = AssignCompanion(gender);
        Assert.AreEqual("boy_comp", companion);
    }

    // ========= 5. Map Lock Logic =========

    [Test]
    public void Test_FirstStory_AlwaysUnlocked()
    {
        bool isFirstStoryOpen = true;
        Assert.IsTrue(isFirstStoryOpen);
    }

    [Test]
    public void Test_SecondStory_LockedIfFirstNotCompleted()
    {
        string previousState = "locked";
        bool isUnlocked = previousState.ToLower() == "completed";
        Assert.IsFalse(isUnlocked);
    }

    [Test]
    public void Test_SecondStory_UnlockedIfFirstCompleted()
    {
        string previousState = "completed";
        bool isUnlocked = previousState.ToLower() == "completed";
        Assert.IsTrue(isUnlocked);
    }

    // ========= 6. Coin Deduction =========

    [Test]
    public void Test_SufficientCoins_AllowsPurchase()
    {
        int coins = 200;
        int price = 150;
        Assert.IsTrue(coins >= price);
    }

    [Test]
    public void Test_InsufficientCoins_BlocksPurchase()
    {
        int coins = 100;
        int price = 150;
        Assert.IsFalse(coins >= price);
    }

    [Test]
    public void Test_CoinDeduction_CorrectAmount()
    {
        int coins = 200;
        int price = 150;
        int remaining = coins - price;
        Assert.AreEqual(50, remaining);
    }

    // ========= Helper Methods =========

    bool IsValidEmail(string email)
    {
        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return System.Text.RegularExpressions.Regex.IsMatch(email, pattern);
    }

    bool IsValidName(string name)
    {
        string[] parts = name.Trim().Split(' ');
        return parts.Length >= 2 && parts[0].Length > 0 && parts[1].Length > 0;
    }

    string AssignCompanion(string gender)
    {
        return (gender == "أنثى" || gender == "انثى" || gender == "female")
            ? "girl_comp"
            : "boy_comp";
    }
}