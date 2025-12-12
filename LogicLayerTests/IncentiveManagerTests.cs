using DataAccessFakes;
using DataDomain;
using LogicLayer;
using LogicLayerInterfaces;

namespace LogicLayerTests;

[TestClass]
public class IncentiveManagerTests
{
    private IIncentiveManager _incentiveManager;
    private IncentiveAccessorFake _incentiveAccessorFake;

    [TestInitialize]
    public void TestInitialize()
    {
        // Arrange: Create fake accessor and inject it into manager
        _incentiveAccessorFake = new IncentiveAccessorFake();
        _incentiveManager = new IncentiveManager(_incentiveAccessorFake);
    }

    // ==================== GetIncentivesByBusinessID Tests ====================

    [TestMethod]
    public void GetIncentivesByBusinessID_WithValidBusinessID_ReturnsIncentives()
    {
        // Arrange
        int businessID = 1;

        // Act
        var result = _incentiveManager.GetIncentivesByBusinessID(businessID);

        // Assert
        Assert.IsNotNull(result, "Result should not be null");
        Assert.IsTrue(result.Count > 0, "Should return at least one incentive");
        Assert.IsTrue(result.All(i => i.BusinessID == businessID),
            "All returned incentives should belong to the specified business");
    }

    [TestMethod]
    public void GetIncentivesByBusinessID_WithNoIncentives_ReturnsEmptyList()
    {
        // Arrange
        _incentiveAccessorFake.ClearFakeData();
        int businessID = 999;

        // Act
        var result = _incentiveManager.GetIncentivesByBusinessID(businessID);

        // Assert
        Assert.IsNotNull(result, "Result should not be null");
        Assert.AreEqual(0, result.Count, "Should return empty list when no incentives found");
    }

    [TestMethod]
    public void GetIncentivesByBusinessID_WhenDatabaseErrorOccurs_ThrowsApplicationException()
    {
        // Arrange
        _incentiveAccessorFake.ShouldThrowException = true;
        int businessID = 1;

        // Act & Assert
        var exception = Assert.ThrowsException<ApplicationException>(() =>
        {
            _incentiveManager.GetIncentivesByBusinessID(businessID);
        });

        Assert.IsTrue(exception.Message.Contains("Error retrieving incentives"),
            "Exception message should indicate retrieval error");
    }

    [TestMethod]
    public void GetIncentivesByBusinessID_ReturnsIncentivesOrderedByDate()
    {
        // Arrange
        int businessID = 1;

        // Act
        var result = _incentiveManager.GetIncentivesByBusinessID(businessID);

        // Assert
        Assert.IsTrue(result.Count > 1, "Need multiple incentives to test ordering");
        for (int i = 0; i < result.Count - 1; i++)
        {
            Assert.IsTrue(result[i].StartDate >= result[i + 1].StartDate,
                "Incentives should be ordered by StartDate descending");
        }
    }

    // ==================== GetIncentiveByID Tests ====================

    [TestMethod]
    public void GetIncentiveByID_WithValidID_ReturnsCorrectIncentive()
    {
        // Arrange
        int incentiveID = 1;

        // Act
        var result = _incentiveManager.GetIncentiveByID(incentiveID);

        // Assert
        Assert.IsNotNull(result, "Result should not be null");
        Assert.AreEqual(incentiveID, result.IncentiveID, "Should return incentive with correct ID");
        Assert.IsNotNull(result.IncentiveDescription, "Description should not be null");
    }

    [TestMethod]
    public void GetIncentiveByID_WithInvalidID_ReturnsNull()
    {
        // Arrange
        _incentiveAccessorFake.ClearFakeData();
        int incentiveID = 999;

        // Act
        var result = _incentiveManager.GetIncentiveByID(incentiveID);

        // Assert
        Assert.IsNull(result, "Result should be null for non-existent ID");
    }

    [TestMethod]
    public void GetIncentiveByID_WhenDatabaseErrorOccurs_ThrowsApplicationException()
    {
        // Arrange
        _incentiveAccessorFake.ShouldThrowException = true;
        int incentiveID = 1;

        // Act & Assert
        var exception = Assert.ThrowsException<ApplicationException>(() =>
        {
            _incentiveManager.GetIncentiveByID(incentiveID);
        });

        Assert.IsTrue(exception.Message.Contains("Error retrieving incentive"),
            "Exception message should indicate retrieval error");
    }

    // ==================== SearchIncentives Tests ====================

    [TestMethod]
    public void SearchIncentives_WithBusinessIDFilter_ReturnsOnlyMatchingBusiness()
    {
        // Arrange
        var criteria = new IncentiveSearchCriteria
        {
            BusinessID = 1,
            ActiveOnly = false // Include all to test filter
        };

        // Act
        var result = _incentiveManager.SearchIncentives(criteria);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Count > 0, "Should find incentives for business 1");
        Assert.IsTrue(result.All(i => i.BusinessID == 1),
            "All results should be from business 1");
    }

    [TestMethod]
    public void SearchIncentives_WithIncentiveTypeFilter_ReturnsOnlyMatchingTypes()
    {
        // Arrange
        var criteria = new IncentiveSearchCriteria
        {
            IncentiveTypeID = "Veteran",
            ActiveOnly = false
        };

        // Act
        var result = _incentiveManager.SearchIncentives(criteria);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Count > 0, "Should find veteran incentives");
        Assert.IsTrue(result.All(i => i.IncentiveTypesDisplay.Contains("Veteran")),
            "All results should include Veteran type");
    }

    [TestMethod]
    public void SearchIncentives_WithMinAmountFilter_ReturnsOnlyAboveMinimum()
    {
        // Arrange
        var criteria = new IncentiveSearchCriteria
        {
            MinAmount = 10.00m,
            ActiveOnly = false
        };

        // Act
        var result = _incentiveManager.SearchIncentives(criteria);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Count > 0, "Should find incentives above $10");
        Assert.IsTrue(result.All(i => i.IncentiveAmount >= 10.00m),
            "All results should have amount >= $10");
    }

    [TestMethod]
    public void SearchIncentives_WithMaxAmountFilter_ReturnsOnlyBelowMaximum()
    {
        // Arrange
        var criteria = new IncentiveSearchCriteria
        {
            MaxAmount = 10.00m,
            ActiveOnly = false
        };

        // Act
        var result = _incentiveManager.SearchIncentives(criteria);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Count > 0, "Should find incentives below $10");
        Assert.IsTrue(result.All(i => i.IncentiveAmount <= 10.00m),
            "All results should have amount <= $10");
    }

    [TestMethod]
    public void SearchIncentives_WithAmountRange_ReturnsOnlyWithinRange()
    {
        // Arrange
        var criteria = new IncentiveSearchCriteria
        {
            MinAmount = 5.00m,
            MaxAmount = 20.00m,
            ActiveOnly = false
        };

        // Act
        var result = _incentiveManager.SearchIncentives(criteria);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.All(i => i.IncentiveAmount >= 5.00m && i.IncentiveAmount <= 20.00m),
            "All results should be within $5-$20 range");
    }

    [TestMethod]
    public void SearchIncentives_WithActiveOnlyTrue_ReturnsOnlyActiveIncentives()
    {
        // Arrange
        var criteria = new IncentiveSearchCriteria
        {
            ActiveOnly = true
        };

        // Act
        var result = _incentiveManager.SearchIncentives(criteria);

        // Assert
        Assert.IsNotNull(result);
        DateTime now = DateTime.Now;
        Assert.IsTrue(result.All(i => i.StartDate <= now && (!i.EndDate.HasValue || i.EndDate.Value >= now)),
            "All results should be currently active");
    }

    [TestMethod]
    public void SearchIncentives_WithActiveOnlyFalse_ReturnsAllIncentives()
    {
        // Arrange
        var criteria = new IncentiveSearchCriteria
        {
            ActiveOnly = false
        };

        // Act
        var result = _incentiveManager.SearchIncentives(criteria);

        // Assert
        Assert.IsNotNull(result);
        // Should include expired incentive (ID 5)
        Assert.IsTrue(result.Any(i => i.IncentiveID == 5),
            "Should include expired incentives when ActiveOnly is false");
    }

    [TestMethod]
    public void SearchIncentives_WithMultipleCriteria_ReturnsIntersectionOfResults()
    {
        // Arrange
        var criteria = new IncentiveSearchCriteria
        {
            BusinessID = 1,
            IncentiveTypeID = "Veteran",
            MinAmount = 0.05m,
            ActiveOnly = true
        };

        // Act
        var result = _incentiveManager.SearchIncentives(criteria);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.All(i =>
            i.BusinessID == 1 &&
            i.IncentiveTypesDisplay.Contains("Veteran") &&
            i.IncentiveAmount >= 0.05m &&
            i.IsCurrentlyActive),
            "All results should match ALL criteria");
    }

    [TestMethod]
    public void SearchIncentives_WithNoMatchingResults_ReturnsEmptyList()
    {
        // Arrange
        _incentiveAccessorFake.ClearFakeData();
        var criteria = new IncentiveSearchCriteria
        {
            BusinessID = 999,
            ActiveOnly = true
        };

        // Act
        var result = _incentiveManager.SearchIncentives(criteria);

        // Assert
        Assert.IsNotNull(result, "Result should not be null");
        Assert.AreEqual(0, result.Count, "Should return empty list when no matches found");
    }

    [TestMethod]
    public void SearchIncentives_WithNullCriteria_ThrowsApplicationException()
    {
        // Arrange
        IncentiveSearchCriteria criteria = null;

        // Act & Assert
        Assert.ThrowsException<ApplicationException>(() =>
        {
            _incentiveManager.SearchIncentives(criteria);
        }, "Should throw ApplicationException when criteria is null");
    }

    [TestMethod]
    public void SearchIncentives_WhenDatabaseErrorOccurs_ThrowsApplicationException()
    {
        // Arrange
        _incentiveAccessorFake.ShouldThrowException = true;
        var criteria = new IncentiveSearchCriteria { ActiveOnly = true };

        // Act & Assert
        var exception = Assert.ThrowsException<ApplicationException>(() =>
        {
            _incentiveManager.SearchIncentives(criteria);
        });

        Assert.IsTrue(exception.Message.Contains("Error searching incentives"),
            "Exception message should indicate search error");
    }

    [TestMethod]
    public void SearchIncentives_WithEmptyCriteria_ReturnsAllActiveIncentives()
    {
        // Arrange
        var criteria = new IncentiveSearchCriteria
        {
            ActiveOnly = true // Default value
        };

        // Act
        var result = _incentiveManager.SearchIncentives(criteria);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Count > 0, "Should return active incentives");
        DateTime now = DateTime.Now;
        Assert.IsTrue(result.All(i => i.StartDate <= now && (!i.EndDate.HasValue || i.EndDate.Value >= now)),
            "All results should be currently active");
    }

    // ==================== Computed Property Tests ====================

    [TestMethod]
    public void Incentive_FormattedAmount_DisplaysCorrectFormat()
    {
        // Arrange
        var percentIncentive = new Incentive
        {
            IncentiveAmount = 15.00m,
            IsPercentage = true
        };
        var dollarIncentive = new Incentive
        {
            IncentiveAmount = 25.00m,
            IsPercentage = false
        };

        // Act & Assert
        Assert.AreEqual("15%", percentIncentive.FormattedAmount,
            "Percentage incentive should display with % symbol");
        Assert.AreEqual("$25.00", dollarIncentive.FormattedAmount,
            "Dollar incentive should display with $ symbol");
    }

    [TestMethod]
    public void Incentive_IsCurrentlyActive_IdentifiesActiveIncentives()
    {
        // Arrange
        var activeIncentive = new Incentive
        {
            StartDate = DateTime.Now.AddMonths(-1),
            EndDate = DateTime.Now.AddMonths(1)
        };

        var expiredIncentive = new Incentive
        {
            StartDate = DateTime.Now.AddMonths(-6),
            EndDate = DateTime.Now.AddMonths(-1)
        };

        var futureIncentive = new Incentive
        {
            StartDate = DateTime.Now.AddMonths(1),
            EndDate = DateTime.Now.AddMonths(2)
        };

        // Act & Assert
        Assert.IsTrue(activeIncentive.IsCurrentlyActive, "Should identify active incentive");
        Assert.IsFalse(expiredIncentive.IsCurrentlyActive, "Should identify expired incentive");
        Assert.IsFalse(futureIncentive.IsCurrentlyActive, "Should identify future incentive");
    }

    [TestMethod]
    public void Incentive_FormattedAmount_WithPercentage_DisplaysPercentSymbol()
    {
        // Arrange
        var incentive = new Incentive
        {
            IncentiveAmount = 15.00m,
            IsPercentage = true
        };

        // Act
        var result = incentive.FormattedAmount;

        // Assert
        Assert.AreEqual("15%", result, "Percentage incentive should display with % symbol");
    }

    [TestMethod]
    public void Incentive_FormattedAmount_WithDecimalPercentage_DisplaysCorrectly()
    {
        // Arrange
        var incentive = new Incentive
        {
            IncentiveAmount = 12.50m,
            IsPercentage = true
        };

        // Act
        var result = incentive.FormattedAmount;

        // Assert
        Assert.AreEqual("12.5%", result, "Should format decimal percentage correctly");
    }

    [TestMethod]
    public void Incentive_FormattedAmount_WithDollarAmount_DisplaysCurrency()
    {
        // Arrange
        var incentive = new Incentive
        {
            IncentiveAmount = 25.00m,
            IsPercentage = false
        };

        // Act
        var result = incentive.FormattedAmount;

        // Assert
        Assert.AreEqual("$25.00", result, "Dollar incentive should display with $ symbol");
    }

    [TestMethod]
    public void Incentive_FormattedAmount_WithSmallDollarAmount_DisplaysCorrectly()
    {
        // Arrange
        var incentive = new Incentive
        {
            IncentiveAmount = 5.00m,
            IsPercentage = false
        };

        // Act
        var result = incentive.FormattedAmount;

        // Assert
        Assert.AreEqual("$5.00", result, "Small dollar amount should display correctly");
    }

    // ==================== AddIncentive Tests ====================

    [TestMethod]
    public void AddIncentive_WithValidIncentive_ReturnsNewIncentiveID()
    {
        // Arrange
        _incentiveAccessorFake.ClearFakeData();
        var newIncentive = new Incentive
        {
            BusinessID = 1,
            IncentiveAmount = 15.00m,
            IsPercentage = true,
            IncentiveDescription = "15% off for veterans",
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddYears(1),
            Limitations = "Dine-in only"
        };
        var incentiveTypes = new List<string> { "Veteran", "Active Duty" };

        // Act
        int newID = _incentiveManager.AddIncentive(newIncentive, incentiveTypes);

        // Assert
        Assert.IsTrue(newID > 0, "Should return a positive IncentiveID");
    }

    [TestMethod]
    public void AddIncentive_WithValidIncentive_IncentiveIsRetrievable()
    {
        // Arrange
        _incentiveAccessorFake.ClearFakeData();
        var newIncentive = new Incentive
        {
            BusinessID = 1,
            IncentiveAmount = 20.00m,
            IsPercentage = false,
            IncentiveDescription = "$20 off service for first responders",
            StartDate = DateTime.Now,
            EndDate = null,
            Limitations = "One per customer"
        };
        var incentiveTypes = new List<string> { "First Responder" };

        // Act
        int newID = _incentiveManager.AddIncentive(newIncentive, incentiveTypes);
        var retrieved = _incentiveManager.GetIncentiveByID(newID);

        // Assert
        Assert.IsNotNull(retrieved, "Should be able to retrieve the added incentive");
        Assert.AreEqual(newIncentive.IncentiveDescription, retrieved.IncentiveDescription);
        Assert.AreEqual(newIncentive.IncentiveAmount, retrieved.IncentiveAmount);
    }

    [TestMethod]
    public void AddIncentive_WithNullIncentive_ThrowsApplicationException()
    {
        // Arrange
        Incentive nullIncentive = null;
        var incentiveTypes = new List<string> { "Veteran" };

        // Act & Assert
        Assert.ThrowsException<ApplicationException>(() =>
        {
            _incentiveManager.AddIncentive(nullIncentive, incentiveTypes);
        }, "Should throw ApplicationException when incentive is null");
    }

    [TestMethod]
    public void AddIncentive_WithZeroBusinessID_ThrowsApplicationException()
    {
        // Arrange
        var incentive = new Incentive
        {
            BusinessID = 0,  // Invalid
            IncentiveAmount = 10.00m,
            IsPercentage = true,
            IncentiveDescription = "Test discount",
            StartDate = DateTime.Now
        };
        var incentiveTypes = new List<string> { "Veteran" };

        // Act & Assert
        Assert.ThrowsException<ApplicationException>(() =>
        {
            _incentiveManager.AddIncentive(incentive, incentiveTypes);
        }, "Should throw ApplicationException when BusinessID is zero");
    }

    [TestMethod]
    public void AddIncentive_WithEmptyDescription_ThrowsApplicationException()
    {
        // Arrange
        var incentive = new Incentive
        {
            BusinessID = 1,
            IncentiveAmount = 10.00m,
            IsPercentage = true,
            IncentiveDescription = "",  // Empty
            StartDate = DateTime.Now
        };
        var incentiveTypes = new List<string> { "Veteran" };

        // Act & Assert
        Assert.ThrowsException<ApplicationException>(() =>
        {
            _incentiveManager.AddIncentive(incentive, incentiveTypes);
        }, "Should throw ApplicationException when description is empty");
    }

    [TestMethod]
    public void AddIncentive_WithNullIncentiveTypes_ThrowsApplicationException()
    {
        // Arrange
        var incentive = new Incentive
        {
            BusinessID = 1,
            IncentiveAmount = 10.00m,
            IsPercentage = true,
            IncentiveDescription = "Test discount",
            StartDate = DateTime.Now
        };
        List<string> nullTypes = null;

        // Act & Assert
        Assert.ThrowsException<ApplicationException>(() =>
        {
            _incentiveManager.AddIncentive(incentive, nullTypes);
        }, "Should throw ApplicationException when incentive types list is null");
    }

    [TestMethod]
    public void AddIncentive_WithEmptyIncentiveTypesList_ThrowsApplicationException()
    {
        // Arrange
        var incentive = new Incentive
        {
            BusinessID = 1,
            IncentiveAmount = 10.00m,
            IsPercentage = true,
            IncentiveDescription = "Test discount",
            StartDate = DateTime.Now
        };
        var emptyTypes = new List<string>();

        // Act & Assert
        Assert.ThrowsException<ApplicationException>(() =>
        {
            _incentiveManager.AddIncentive(incentive, emptyTypes);
        }, "Should throw ApplicationException when incentive types list is empty");
    }

    [TestMethod]
    public void AddIncentive_WithNegativeAmount_ThrowsApplicationException()
    {
        // Arrange
        var incentive = new Incentive
        {
            BusinessID = 1,
            IncentiveAmount = -5.00m,  // Negative
            IsPercentage = true,
            IncentiveDescription = "Test discount",
            StartDate = DateTime.Now
        };
        var incentiveTypes = new List<string> { "Veteran" };

        // Act & Assert
        Assert.ThrowsException<ApplicationException>(() =>
        {
            _incentiveManager.AddIncentive(incentive, incentiveTypes);
        }, "Should throw ApplicationException when amount is negative");
    }

    [TestMethod]
    public void AddIncentive_WhenDatabaseErrorOccurs_ThrowsApplicationException()
    {
        // Arrange
        _incentiveAccessorFake.ShouldThrowException = true;
        var incentive = new Incentive
        {
            BusinessID = 1,
            IncentiveAmount = 10.00m,
            IsPercentage = true,
            IncentiveDescription = "Test discount",
            StartDate = DateTime.Now
        };
        var incentiveTypes = new List<string> { "Veteran" };

        // Act & Assert
        var exception = Assert.ThrowsException<ApplicationException>(() =>
        {
            _incentiveManager.AddIncentive(incentive, incentiveTypes);
        });

        Assert.IsTrue(exception.Message.Contains("Error adding incentive"),
            "Exception message should indicate add error");
    }

    [TestMethod]
    public void AddIncentive_WithMultipleIncentiveTypes_SetsIncentiveTypesDisplay()
    {
        // Arrange
        _incentiveAccessorFake.ClearFakeData();
        var incentive = new Incentive
        {
            BusinessID = 1,
            IncentiveAmount = 25.00m,
            IsPercentage = true,
            IncentiveDescription = "25% discount for military",
            StartDate = DateTime.Now
        };
        var incentiveTypes = new List<string> { "Veteran", "Active Duty", "Spouse" };

        // Act
        int newID = _incentiveManager.AddIncentive(incentive, incentiveTypes);
        var retrieved = _incentiveManager.GetIncentiveByID(newID);

        // Assert
        Assert.IsNotNull(retrieved.IncentiveTypesDisplay);
        Assert.IsTrue(retrieved.IncentiveTypesDisplay.Contains("Veteran"));
        Assert.IsTrue(retrieved.IncentiveTypesDisplay.Contains("Active Duty"));
        Assert.IsTrue(retrieved.IncentiveTypesDisplay.Contains("Spouse"));
    }

    [TestMethod]
    public void AddIncentive_WithOptionalEndDateNull_SuccessfullyAdds()
    {
        // Arrange
        _incentiveAccessorFake.ClearFakeData();
        var incentive = new Incentive
        {
            BusinessID = 1,
            IncentiveAmount = 10.00m,
            IsPercentage = true,
            IncentiveDescription = "Ongoing discount",
            StartDate = DateTime.Now,
            EndDate = null  // No end date (ongoing)
        };
        var incentiveTypes = new List<string> { "Veteran" };

        // Act
        int newID = _incentiveManager.AddIncentive(incentive, incentiveTypes);
        var retrieved = _incentiveManager.GetIncentiveByID(newID);

        // Assert
        Assert.IsTrue(newID > 0);
        Assert.IsNull(retrieved.EndDate, "EndDate should remain null");
    }

    [TestMethod]
    public void AddIncentive_WithOptionalLimitationsEmpty_SuccessfullyAdds()
    {
        // Arrange
        _incentiveAccessorFake.ClearFakeData();
        var incentive = new Incentive
        {
            BusinessID = 1,
            IncentiveAmount = 15.00m,
            IsPercentage = false,
            IncentiveDescription = "$15 off",
            StartDate = DateTime.Now,
            Limitations = ""  // Empty limitations
        };
        var incentiveTypes = new List<string> { "First Responder" };

        // Act
        int newID = _incentiveManager.AddIncentive(incentive, incentiveTypes);

        // Assert
        Assert.IsTrue(newID > 0);
    }

    [TestMethod]
    public void AddIncentive_SetsCreatedAtAndLastUpdated()
    {
        // Arrange
        _incentiveAccessorFake.ClearFakeData();
        var beforeAdd = DateTime.Now.AddSeconds(-1);
        var incentive = new Incentive
        {
            BusinessID = 1,
            IncentiveAmount = 10.00m,
            IsPercentage = true,
            IncentiveDescription = "Test discount",
            StartDate = DateTime.Now
        };
        var incentiveTypes = new List<string> { "Veteran" };

        // Act
        int newID = _incentiveManager.AddIncentive(incentive, incentiveTypes);
        var retrieved = _incentiveManager.GetIncentiveByID(newID);
        var afterAdd = DateTime.Now.AddSeconds(1);

        // Assert
        Assert.IsTrue(retrieved.CreatedAt >= beforeAdd && retrieved.CreatedAt <= afterAdd,
            "CreatedAt should be set to current time");
        Assert.IsTrue(retrieved.LastUpdated >= beforeAdd && retrieved.LastUpdated <= afterAdd,
            "LastUpdated should be set to current time");
    }

    // ==================== UpdateIncentive Tests ====================

    [TestMethod]
    public void UpdateIncentive_WithValidIncentive_ReturnsTrue()
    {
        // Arrange
        var updatedIncentive = new Incentive
        {
            IncentiveID = 1,  // Existing incentive
            BusinessID = 1,
            IncentiveAmount = 20.00m,  // Changed from 10.00
            IsPercentage = true,
            IncentiveDescription = "Updated: 20% off entire meal",
            StartDate = DateTime.Now.AddMonths(-6),
            EndDate = DateTime.Now.AddMonths(12),
            Limitations = "Updated limitations"
        };
        var incentiveTypes = new List<string> { "Veteran", "Active Duty", "Spouse" };

        // Act
        bool result = _incentiveManager.UpdateIncentive(updatedIncentive, incentiveTypes);

        // Assert
        Assert.IsTrue(result, "Update should return true for successful update");
    }

    [TestMethod]
    public void UpdateIncentive_WithValidIncentive_PersistsChanges()
    {
        // Arrange
        var updatedIncentive = new Incentive
        {
            IncentiveID = 1,
            BusinessID = 1,
            IncentiveAmount = 25.00m,
            IsPercentage = false,  // Changed from percentage to dollar
            IncentiveDescription = "Now $25 off instead of percentage",
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddYears(2),
            Limitations = "New limitations apply"
        };
        var incentiveTypes = new List<string> { "First Responder" };

        // Act
        _incentiveManager.UpdateIncentive(updatedIncentive, incentiveTypes);
        var retrieved = _incentiveManager.GetIncentiveByID(1);

        // Assert
        Assert.IsNotNull(retrieved);
        Assert.AreEqual(25.00m, retrieved.IncentiveAmount);
        Assert.IsFalse(retrieved.IsPercentage);
        Assert.AreEqual("Now $25 off instead of percentage", retrieved.IncentiveDescription);
        Assert.IsTrue(retrieved.IncentiveTypesDisplay.Contains("First Responder"));
    }

    [TestMethod]
    public void UpdateIncentive_WithNonExistentID_ReturnsFalse()
    {
        // Arrange
        var nonExistentIncentive = new Incentive
        {
            IncentiveID = 9999,  // Non-existent
            BusinessID = 1,
            IncentiveAmount = 10.00m,
            IsPercentage = true,
            IncentiveDescription = "Test discount",
            StartDate = DateTime.Now
        };
        var incentiveTypes = new List<string> { "Veteran" };

        // Act
        bool result = _incentiveManager.UpdateIncentive(nonExistentIncentive, incentiveTypes);

        // Assert
        Assert.IsFalse(result, "Update should return false for non-existent incentive");
    }

    [TestMethod]
    public void UpdateIncentive_WithNullIncentive_ThrowsApplicationException()
    {
        // Arrange
        Incentive nullIncentive = null;
        var incentiveTypes = new List<string> { "Veteran" };

        // Act & Assert
        Assert.ThrowsException<ApplicationException>(() =>
        {
            _incentiveManager.UpdateIncentive(nullIncentive, incentiveTypes);
        }, "Should throw ApplicationException when incentive is null");
    }

    [TestMethod]
    public void UpdateIncentive_WithZeroBusinessID_ThrowsApplicationException()
    {
        // Arrange
        var incentive = new Incentive
        {
            IncentiveID = 1,
            BusinessID = 0,  // Invalid
            IncentiveAmount = 10.00m,
            IsPercentage = true,
            IncentiveDescription = "Test discount",
            StartDate = DateTime.Now
        };
        var incentiveTypes = new List<string> { "Veteran" };

        // Act & Assert
        Assert.ThrowsException<ApplicationException>(() =>
        {
            _incentiveManager.UpdateIncentive(incentive, incentiveTypes);
        }, "Should throw ApplicationException when BusinessID is zero");
    }

    [TestMethod]
    public void UpdateIncentive_WithEmptyDescription_ThrowsApplicationException()
    {
        // Arrange
        var incentive = new Incentive
        {
            IncentiveID = 1,
            BusinessID = 1,
            IncentiveAmount = 10.00m,
            IsPercentage = true,
            IncentiveDescription = "",  // Empty
            StartDate = DateTime.Now
        };
        var incentiveTypes = new List<string> { "Veteran" };

        // Act & Assert
        Assert.ThrowsException<ApplicationException>(() =>
        {
            _incentiveManager.UpdateIncentive(incentive, incentiveTypes);
        }, "Should throw ApplicationException when description is empty");
    }

    [TestMethod]
    public void UpdateIncentive_WithNullIncentiveTypes_ThrowsApplicationException()
    {
        // Arrange
        var incentive = new Incentive
        {
            IncentiveID = 1,
            BusinessID = 1,
            IncentiveAmount = 10.00m,
            IsPercentage = true,
            IncentiveDescription = "Test discount",
            StartDate = DateTime.Now
        };
        List<string> nullTypes = null;

        // Act & Assert
        Assert.ThrowsException<ApplicationException>(() =>
        {
            _incentiveManager.UpdateIncentive(incentive, nullTypes);
        }, "Should throw ApplicationException when incentive types list is null");
    }

    [TestMethod]
    public void UpdateIncentive_WithEmptyIncentiveTypesList_ThrowsApplicationException()
    {
        // Arrange
        var incentive = new Incentive
        {
            IncentiveID = 1,
            BusinessID = 1,
            IncentiveAmount = 10.00m,
            IsPercentage = true,
            IncentiveDescription = "Test discount",
            StartDate = DateTime.Now
        };
        var emptyTypes = new List<string>();

        // Act & Assert
        Assert.ThrowsException<ApplicationException>(() =>
        {
            _incentiveManager.UpdateIncentive(incentive, emptyTypes);
        }, "Should throw ApplicationException when incentive types list is empty");
    }

    [TestMethod]
    public void UpdateIncentive_WithNegativeAmount_ThrowsApplicationException()
    {
        // Arrange
        var incentive = new Incentive
        {
            IncentiveID = 1,
            BusinessID = 1,
            IncentiveAmount = -5.00m,  // Negative
            IsPercentage = true,
            IncentiveDescription = "Test discount",
            StartDate = DateTime.Now
        };
        var incentiveTypes = new List<string> { "Veteran" };

        // Act & Assert
        Assert.ThrowsException<ApplicationException>(() =>
        {
            _incentiveManager.UpdateIncentive(incentive, incentiveTypes);
        }, "Should throw ApplicationException when amount is negative");
    }

    [TestMethod]
    public void UpdateIncentive_WhenDatabaseErrorOccurs_ThrowsApplicationException()
    {
        // Arrange
        _incentiveAccessorFake.ShouldThrowException = true;
        var incentive = new Incentive
        {
            IncentiveID = 1,
            BusinessID = 1,
            IncentiveAmount = 10.00m,
            IsPercentage = true,
            IncentiveDescription = "Test discount",
            StartDate = DateTime.Now
        };
        var incentiveTypes = new List<string> { "Veteran" };

        // Act & Assert
        var exception = Assert.ThrowsException<ApplicationException>(() =>
        {
            _incentiveManager.UpdateIncentive(incentive, incentiveTypes);
        });

        Assert.IsTrue(exception.Message.Contains("Error updating incentive"),
            "Exception message should indicate update error");
    }

    [TestMethod]
    public void UpdateIncentive_ChangingIncentiveTypes_UpdatesTypesCorrectly()
    {
        // Arrange - Incentive 1 starts with "Active Duty, Veteran"
        var updatedIncentive = new Incentive
        {
            IncentiveID = 1,
            BusinessID = 1,
            IncentiveAmount = 10.00m,
            IsPercentage = true,
            IncentiveDescription = "10% off entire meal for active duty",
            StartDate = DateTime.Now.AddMonths(-6),
            EndDate = DateTime.Now.AddMonths(6),
            Limitations = "Valid Monday-Thursday only."
        };
        // Change from "Active Duty, Veteran" to just "First Responder"
        var newIncentiveTypes = new List<string> { "First Responder" };

        // Act
        _incentiveManager.UpdateIncentive(updatedIncentive, newIncentiveTypes);
        var retrieved = _incentiveManager.GetIncentiveByID(1);

        // Assert
        Assert.IsNotNull(retrieved);
        Assert.IsTrue(retrieved.IncentiveTypesDisplay.Contains("First Responder"));
        Assert.IsFalse(retrieved.IncentiveTypesDisplay.Contains("Active Duty"));
        Assert.IsFalse(retrieved.IncentiveTypesDisplay.Contains("Veteran"));
    }

    [TestMethod]
    public void UpdateIncentive_WithOptionalEndDateNull_SuccessfullyUpdates()
    {
        // Arrange
        var updatedIncentive = new Incentive
        {
            IncentiveID = 1,
            BusinessID = 1,
            IncentiveAmount = 10.00m,
            IsPercentage = true,
            IncentiveDescription = "Ongoing discount - no end date",
            StartDate = DateTime.Now,
            EndDate = null  // Clear the end date
        };
        var incentiveTypes = new List<string> { "Veteran" };

        // Act
        bool result = _incentiveManager.UpdateIncentive(updatedIncentive, incentiveTypes);
        var retrieved = _incentiveManager.GetIncentiveByID(1);

        // Assert
        Assert.IsTrue(result);
        Assert.IsNull(retrieved.EndDate, "EndDate should be null after update");
    }

    [TestMethod]
    public void UpdateIncentive_SetsLastUpdatedToCurrentTime()
    {
        // Arrange
        var beforeUpdate = DateTime.Now.AddSeconds(-1);
        var updatedIncentive = new Incentive
        {
            IncentiveID = 1,
            BusinessID = 1,
            IncentiveAmount = 15.00m,
            IsPercentage = true,
            IncentiveDescription = "Updated discount",
            StartDate = DateTime.Now
        };
        var incentiveTypes = new List<string> { "Veteran" };

        // Act
        _incentiveManager.UpdateIncentive(updatedIncentive, incentiveTypes);
        var afterUpdate = DateTime.Now.AddSeconds(1);
        var retrieved = _incentiveManager.GetIncentiveByID(1);

        // Assert
        Assert.IsTrue(retrieved.LastUpdated >= beforeUpdate && retrieved.LastUpdated <= afterUpdate,
            "LastUpdated should be set to current time");
    }

    [TestMethod]
    public void UpdateIncentive_DoesNotChangeCreatedAt()
    {
        // Arrange
        var originalIncentive = _incentiveManager.GetIncentiveByID(1);
        var originalCreatedAt = originalIncentive.CreatedAt;

        var updatedIncentive = new Incentive
        {
            IncentiveID = 1,
            BusinessID = 1,
            IncentiveAmount = 99.00m,
            IsPercentage = false,
            IncentiveDescription = "Completely different discount",
            StartDate = DateTime.Now
        };
        var incentiveTypes = new List<string> { "Spouse" };

        // Act
        _incentiveManager.UpdateIncentive(updatedIncentive, incentiveTypes);
        var retrieved = _incentiveManager.GetIncentiveByID(1);

        // Assert
        Assert.AreEqual(originalCreatedAt, retrieved.CreatedAt,
            "CreatedAt should not change during update");
    }
}
