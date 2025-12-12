using LogicLayer;
using LogicLayerInterfaces;
using DataAccessFakes;
using DataAccessInterfaces;
using System.ComponentModel.DataAnnotations;
using DataDomain;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace LogicLayerTests;

[TestClass]
public sealed class BusinessManagerTests
{
    private IBusinessManager _businessManager;
    private BusinessAccessorFake _businessAccessorFake;
    private IncentiveAccessorFake _incentiveAccessorFake;
   

    [TestInitialize]
    public void TestInitialize()
    {
        // Arrange: Create fake accessor and inject it into manager
        _businessAccessorFake = new BusinessAccessorFake();
        _businessManager = new BusinessManager(_businessAccessorFake);
        _incentiveAccessorFake = new IncentiveAccessorFake();
    }


    [TestMethod]
    public void SearchBusinesses_WithNoMatchingResults_ReturnsEmptyList()
    {
        // Arrange

        _businessAccessorFake.ClearFakeData();
        var criteria = new BusinessSearchCriteria
        {
            BusinessName = "NonExistentBusiness",
            IsActive = true
        };

        // Act
        var result = _businessManager.SearchBusinesses(criteria);

        // Assert
        Assert.IsNotNull(result, "Result should not be null");
        Assert.AreEqual(0, result.Count, "Should return empty list when no matches found");
    }

    [TestMethod]
    public void SearchBusinesses_WithMatchingBusinessName_ReturnsCorrectBusinesses()
    {
        // Arrange

        _businessAccessorFake.ClearFakeData();
        _businessAccessorFake.AddFakeBusiness(new Business
        {
            BusinessID = 1,
            BusinessName = "Joe's Pizza",
            BusinessTypeID = "Restaurant",
            IsActive = true,
            City = "Cedar Rapids",
            StateID = "IA"
        });

        _businessAccessorFake.AddFakeBusiness(new Business
        {
            BusinessID = 2,
            BusinessName = "Joe's Burgers",
            BusinessTypeID = "Restaurant",
            IsActive = true,
            City = "Iowa City",
            StateID = "IA"
        });

        var criteria = new BusinessSearchCriteria
        {
            BusinessName = "Joe",
            IsActive = true
        };

        // Act
        var result = _businessManager.SearchBusinesses(criteria);

        // Assert
        Assert.IsNotNull(result, "Result should not be null");
        Assert.AreEqual(2, result.Count, "Should return 2 businesses matching 'Joe'");
        Assert.IsTrue(result[0].BusinessName.Contains("Joe"), "First result should contain 'Joe'");
        Assert.IsTrue(result[1].BusinessName.Contains("Joe"), "Second result should contain 'Joe'");
    }

    [TestMethod]
    public void SearchBusinesses_WithCityFilter_ReturnsOnlyBusinessesInThatCity()
    {
        // Arrange

        _businessAccessorFake.ClearFakeData();
        _businessAccessorFake.AddFakeBusiness(new Business
        {
            BusinessID = 1,
            BusinessName = "Cedar Rapids Store",
            City = "Cedar Rapids",
            StateID = "IA",
            IsActive = true
        });

        _businessAccessorFake.AddFakeBusiness(new Business
        {
            BusinessID = 2,
            BusinessName = "Iowa City Store",
            City = "Iowa City",
            StateID = "IA",
            IsActive = true
        });

        var criteria = new BusinessSearchCriteria
        {
            City = "Cedar Rapids",
            IsActive = true
        };

        // Act
        var result = _businessManager.SearchBusinesses(criteria);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count, "Should return only businesses in Cedar Rapids");
        Assert.AreEqual("Cedar Rapids", result[0].City);
    }

    [TestMethod]
    public void SearchBusinesses_WithStateFilter_ReturnsOnlyBusinessesInThatState()
    {
        // Arrange

        _businessAccessorFake.ClearFakeData();
        _businessAccessorFake.AddFakeBusiness(new Business
        {
            BusinessID = 1,
            BusinessName = "Iowa Store",
            StateID = "IA",
            IsActive = true
        });

        _businessAccessorFake.AddFakeBusiness(new Business
        {
            BusinessID = 2,
            BusinessName = "Illinois Store",
            StateID = "IL",
            IsActive = true
        });

        var criteria = new BusinessSearchCriteria
        {
            StateID = "IA",
            IsActive = true
        };

        // Act
        var result = _businessManager.SearchBusinesses(criteria);

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("IA", result[0].StateID);
    }

    [TestMethod]
    public void SearchBusinesses_WithZipFilter_ReturnsOnlyBusinessesWithMatchingZip()
    {
        // Arrange

        _businessAccessorFake.ClearFakeData();
        _businessAccessorFake.AddFakeBusiness(new Business
        {
            BusinessID = 1,
            BusinessName = "Business in 52402",
            Zip = "52402",
            IsActive = true
        });

        _businessAccessorFake.AddFakeBusiness(new Business
        {
            BusinessID = 2,
            BusinessName = "Business in 52404",
            Zip = "52404",
            IsActive = true
        });

        var criteria = new BusinessSearchCriteria
        {
            Zip = "52402",
            IsActive = true
        };

        // Act
        var result = _businessManager.SearchBusinesses(criteria);

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("52402", result[0].Zip);
    }

    [TestMethod]
    public void SearchBusinesses_WithBusinessTypeFilter_ReturnsOnlyMatchingBusinessTypes()
    {
        // Arrange
        _businessAccessorFake.ClearFakeData();
        _businessAccessorFake.AddFakeBusiness(new Business
        {
            BusinessID = 1,
            BusinessName = "Pizza Place",
            BusinessTypeID = "Restaurant",
            IsActive = true
        });

        _businessAccessorFake.AddFakeBusiness(new Business
        {
            BusinessID = 2,
            BusinessName = "Car Shop",
            BusinessTypeID = "Automotive",
            IsActive = true
        });

        var criteria = new BusinessSearchCriteria
        {
            BusinessTypeID = "Restaurant",
            IsActive = true
        };

        // Act
        var result = _businessManager.SearchBusinesses(criteria);

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Restaurant", result[0].BusinessTypeID);
    }

    [TestMethod]
    public void SearchBusinesses_WithMultipleCriteria_ReturnsIntersectionOfResults()
    {
        // Arrange

        _businessAccessorFake.ClearFakeData();
        _businessAccessorFake.AddFakeBusiness(new Business
        {
            BusinessID = 1,
            BusinessName = "Joe's Pizza",
            BusinessTypeID = "Restaurant",
            City = "Cedar Rapids",
            StateID = "IA",
            IsActive = true
        });

        _businessAccessorFake.AddFakeBusiness(new Business
        {
            BusinessID = 2,
            BusinessName = "Joe's Burgers",
            BusinessTypeID = "Restaurant",
            City = "Iowa City",
            StateID = "IA",
            IsActive = true
        });

        _businessAccessorFake.AddFakeBusiness(new Business
        {
            BusinessID = 3,
            BusinessName = "Mike's Pizza",
            BusinessTypeID = "Restaurant",
            City = "Cedar Rapids",
            StateID = "IA",
            IsActive = true
        });

        var criteria = new BusinessSearchCriteria
        {
            BusinessName = "Joe",
            City = "Cedar Rapids",
            IsActive = true
        };

        // Act
        var result = _businessManager.SearchBusinesses(criteria);

        // Assert
        Assert.AreEqual(1, result.Count, "Should return only businesses matching ALL criteria");
        Assert.AreEqual("Joe's Pizza", result[0].BusinessName);
        Assert.AreEqual("Cedar Rapids", result[0].City);
    }

    [TestMethod]
    public void SearchBusinesses_WithIsActiveFalse_ReturnsInactiveBusinesses()
    {
        // Arrange
        _businessAccessorFake.ClearFakeData();
        _businessAccessorFake.AddFakeBusiness(new Business
        {
            BusinessID = 1,
            BusinessName = "Active Business",
            IsActive = true
        });

        _businessAccessorFake.AddFakeBusiness(new Business
        {
            BusinessID = 2,
            BusinessName = "Inactive Business",
            IsActive = false
        });

        var criteria = new BusinessSearchCriteria
        {
            IsActive = false
        };

        // Act
        var result = _businessManager.SearchBusinesses(criteria);

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.IsFalse(result[0].IsActive);
    }

    [TestMethod]
    public void SearchBusinesses_WithNullCriteria_ThrowsApplicationException()
    {
        // Arrange
        _businessAccessorFake.ClearFakeData();
        BusinessSearchCriteria criteria = null;

        // Act & Assert
        Assert.ThrowsException<ApplicationException>(() =>
        {
            _businessManager.SearchBusinesses(criteria);
        }, "Should throw ApplicationException when criteria is null");
    }

    [TestMethod]
    public void SearchBusinesses_WhenDatabaseErrorOccurs_ThrowsApplicationException()
    {
        // Arrange
        _businessAccessorFake.ClearFakeData();
        _businessAccessorFake.ShouldThrowException = true;
        var criteria = new BusinessSearchCriteria { IsActive = true };

        // Act & Assert
        var exception = Assert.ThrowsException<ApplicationException>(() =>
        {
            _businessManager.SearchBusinesses(criteria);
        });

        Assert.IsTrue(exception.Message.Contains("Error searching businesses"),
            "Exception message should indicate search error");
    }

    [TestMethod]
    public void SearchBusinesses_WithEmptyCriteria_ReturnsAllActiveBusinesses()
    {
        // Arrange
        _businessAccessorFake.ClearFakeData();
        _businessAccessorFake.AddFakeBusiness(new Business
        {
            BusinessID = 1,
            BusinessName = "Business 1",
            IsActive = true
        });

        _businessAccessorFake.AddFakeBusiness(new Business
        {
            BusinessID = 2,
            BusinessName = "Business 2",
            IsActive = true
        });

        _businessAccessorFake.AddFakeBusiness(new Business
        {
            BusinessID = 3,
            BusinessName = "Inactive Business",
            IsActive = false
        });

        var criteria = new BusinessSearchCriteria
        {
            IsActive = true
        };

        // Act
        var result = _businessManager.SearchBusinesses(criteria);

        // Assert
        Assert.AreEqual(2, result.Count, "Should return all active businesses");
    }

    // ==================== GetBusinessByID Tests ====================

    [TestMethod]
    public void GetBusinessByID_WithValidID_ReturnsCorrectBusiness()
    {
        // Arrange
        _businessAccessorFake.AddFakeBusiness(new Business
        {
            BusinessID = 100,
            BusinessName = "Specific Business",
            BusinessTypeID = "Restaurant",
            City = "Cedar Rapids",
            StateID = "IA",
            IsActive = true
        });

        // Act
        var result = _businessManager.GetBusinessByID(100);

        // Assert
        Assert.IsNotNull(result, "Result should not be null");
        Assert.AreEqual(100, result.BusinessID);
        Assert.AreEqual("Specific Business", result.BusinessName);
    }

    [TestMethod]
    public void GetBusinessByID_WithInvalidID_ReturnsNull()
    {
        // Arrange - no business with ID 999 exists
        _businessAccessorFake.ClearFakeData();

        // Act
        var result = _businessManager.GetBusinessByID(999);

        // Assert
        Assert.IsNull(result, "Result should be null for non-existent ID");
    }

    [TestMethod]
    public void GetBusinessByID_WhenDatabaseErrorOccurs_ThrowsApplicationException()
    {
        // Arrange
        _businessAccessorFake.ClearFakeData();
        _businessAccessorFake.ShouldThrowException = true;

        // Act & Assert
        var exception = Assert.ThrowsException<ApplicationException>(() =>
        {
            _businessManager.GetBusinessByID(1);
        });

        Assert.IsTrue(exception.Message.Contains("Error retrieving business"),
            "Exception message should indicate retrieval error");
    }

    // ==================== UpdateBusiness Tests ====================

    [TestMethod]
    public void UpdateBusiness_WithValidBusiness_ReturnsTrue()
    {
        // Arrange
        _businessAccessorFake.ClearFakeData();
        _businessAccessorFake.AddFakeBusiness(new Business
        {
            BusinessID = 100,
            BusinessName = "Original Name",
            BusinessTypeID = "Restaurant",
            City = "Cedar Rapids",
            StateID = "IA",
            IsActive = true
        });

        var updatedBusiness = new Business
        {
            BusinessID = 100,
            BusinessName = "Updated Name",
            BusinessTypeID = "Restaurant",
            Phone = "(319) 555-9999",
            City = "Cedar Rapids",
            StateID = "IA",
            IsActive = true
        };

        // Act
        var result = _businessManager.UpdateBusiness(updatedBusiness);

        // Assert
        Assert.IsTrue(result, "Update should return true for successful update");

        // Verify the update persisted
        var verifyBusiness = _businessManager.GetBusinessByID(100);
        Assert.AreEqual("Updated Name", verifyBusiness.BusinessName);
    }

    [TestMethod]
    public void UpdateBusiness_WithNonExistentID_ReturnsFalse()
    {
        // Arrange
        _businessAccessorFake.ClearFakeData();
        var business = new Business
        {
            BusinessID = 999,
            BusinessName = "Non-existent Business",
            IsActive = true
        };

        // Act
        var result = _businessManager.UpdateBusiness(business);

        // Assert
        Assert.IsFalse(result, "Update should return false for non-existent business");
    }

    [TestMethod]
    public void UpdateBusiness_WithNullBusiness_ThrowsApplicationException()
    {
        // Arrange
        _businessAccessorFake.ClearFakeData();
        Business business = null;

        // Act & Assert
        Assert.ThrowsException<ApplicationException>(() =>
        {
            _businessManager.UpdateBusiness(business);
        }, "Should throw ApplicationException when business is null");
    }

    [TestMethod]
    public void UpdateBusiness_WhenDatabaseErrorOccurs_ThrowsApplicationException()
    {
        // Arrange
        _businessAccessorFake.ClearFakeData();
        _businessAccessorFake.ShouldThrowException = true;
        var business = new Business
        {
            BusinessID = 1,
            BusinessName = "Test",
            IsActive = true
        };

        // Act & Assert
        var exception = Assert.ThrowsException<ApplicationException>(() =>
        {
            _businessManager.UpdateBusiness(business);
        });

        Assert.IsTrue(exception.Message.Contains("Error updating business"),
            "Exception message should indicate update error");
    }

    // ==================== AddBusiness Tests ====================

    [TestMethod]
    public void AddBusiness_WithValidBusiness_ReturnsNewBusinessID()
    {
        // Arrange
        _businessAccessorFake.ClearFakeData();
        var newBusiness = new Business
        {
            BusinessName = "New Test Business",
            BusinessTypeID = "Restaurant",
            Phone = "(319) 555-1234",
            StreetAddress = "123 New Street",
            Address2 = "Suite 100",
            City = "Cedar Rapids",
            StateID = "IA",
            Zip = "52402"
        };

        // Act
        int newID = _businessManager.AddBusiness(newBusiness);

        // Assert
        Assert.IsTrue(newID > 0, "Should return a positive BusinessID");

        // Verify the business was actually added
        var criteria = new BusinessSearchCriteria
        {
            BusinessName = "New Test Business",
            IsActive = true
        };
        var results = _businessManager.SearchBusinesses(criteria);
        Assert.AreEqual(1, results.Count, "Business should be searchable after adding");
    }

    [TestMethod]
    public void AddBusiness_WithNullBusiness_ThrowsApplicationException()
    {
        // Arrange
        _businessAccessorFake.ClearFakeData();
        Business business = null;

        // Act & Assert
        Assert.ThrowsException<ApplicationException>(() =>
        {
            _businessManager.AddBusiness(business);
        }, "Should throw ApplicationException when business is null");
    }

    [TestMethod]
    public void AddBusiness_WithMissingBusinessName_ThrowsApplicationException()
    {
        // Arrange
        _businessAccessorFake.ClearFakeData();
        var business = new Business
        {
            BusinessName = "",  // Empty name
            BusinessTypeID = "Restaurant",
            StreetAddress = "123 Main St",
            City = "Cedar Rapids",
            StateID = "IA",
            Zip = "52402"
        };

        // Act & Assert
        Assert.ThrowsException<ApplicationException>(() =>
        {
            _businessManager.AddBusiness(business);
        }, "Should throw ApplicationException when business name is missing");
    }

    [TestMethod]
    public void AddBusiness_WithMissingBusinessType_ThrowsApplicationException()
    {
        // Arrange
        _businessAccessorFake.ClearFakeData();
        var business = new Business
        {
            BusinessName = "Test Business",
            BusinessTypeID = "",  // Empty type
            StreetAddress = "123 Main St",
            City = "Cedar Rapids",
            StateID = "IA",
            Zip = "52402"
        };

        // Act & Assert
        Assert.ThrowsException<ApplicationException>(() =>
        {
            _businessManager.AddBusiness(business);
        }, "Should throw ApplicationException when business type is missing");
    }

    [TestMethod]
    public void AddBusiness_WithMissingStreetAddress_ThrowsApplicationException()
    {
        // Arrange
        _businessAccessorFake.ClearFakeData();
        var business = new Business
        {
            BusinessName = "Test Business",
            BusinessTypeID = "Restaurant",
            StreetAddress = "",  // Empty address
            City = "Cedar Rapids",
            StateID = "IA",
            Zip = "52402"
        };

        // Act & Assert
        Assert.ThrowsException<ApplicationException>(() =>
        {
            _businessManager.AddBusiness(business);
        }, "Should throw ApplicationException when street address is missing");
    }

    [TestMethod]
    public void AddBusiness_WithMissingCity_ThrowsApplicationException()
    {
        // Arrange
        _businessAccessorFake.ClearFakeData();
        var business = new Business
        {
            BusinessName = "Test Business",
            BusinessTypeID = "Restaurant",
            StreetAddress = "123 Main St",
            City = "",  // Empty city
            StateID = "IA",
            Zip = "52402"
        };

        // Act & Assert
        Assert.ThrowsException<ApplicationException>(() =>
        {
            _businessManager.AddBusiness(business);
        }, "Should throw ApplicationException when city is missing");
    }

    [TestMethod]
    public void AddBusiness_WithMissingState_ThrowsApplicationException()
    {
        // Arrange
        _businessAccessorFake.ClearFakeData();
        var business = new Business
        {
            BusinessName = "Test Business",
            BusinessTypeID = "Restaurant",
            StreetAddress = "123 Main St",
            City = "Cedar Rapids",
            StateID = "",  // Empty state
            Zip = "52402"
        };

        // Act & Assert
        Assert.ThrowsException<ApplicationException>(() =>
        {
            _businessManager.AddBusiness(business);
        }, "Should throw ApplicationException when state is missing");
    }

    [TestMethod]
    public void AddBusiness_WhenDatabaseErrorOccurs_ThrowsApplicationException()
    {
        // Arrange
        _businessAccessorFake.ClearFakeData();
        _businessAccessorFake.ShouldThrowException = true;
        var business = new Business
        {
            BusinessName = "Test Business",
            BusinessTypeID = "Restaurant",
            StreetAddress = "123 Main St",
            City = "Cedar Rapids",
            StateID = "IA",
            Zip = "52402"
        };

        // Act & Assert
        var exception = Assert.ThrowsException<ApplicationException>(() =>
        {
            _businessManager.AddBusiness(business);
        });

        Assert.IsTrue(exception.Message.Contains("Error adding business"),
            "Exception message should indicate add error");
    }

    [TestMethod]
    public void AddBusiness_SetsIsActiveToTrue()
    {
        // Arrange
        _businessAccessorFake.ClearFakeData();
        var newBusiness = new Business
        {
            BusinessName = "Active Business Test",
            BusinessTypeID = "Restaurant",
            StreetAddress = "456 Test Ave",
            City = "Marion",
            StateID = "IA",
            Zip = "52302",
            IsActive = false  // Set to false, should be overridden
        };

        // Act
        int newID = _businessManager.AddBusiness(newBusiness);

        // Assert - search with IsActive = true should find it
        var criteria = new BusinessSearchCriteria
        {
            BusinessName = "Active Business Test",
            IsActive = true
        };
        var results = _businessManager.SearchBusinesses(criteria);
        Assert.AreEqual(1, results.Count, "New business should be active");
    }

    

}










