using LogicLayer;
using LogicLayerInterfaces;
using DataAccessFakes;
using DataAccessInterfaces;
using System.ComponentModel.DataAnnotations;
using DataDomain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LogicLayerTests;

[TestClass]
public sealed class UserManagerTests
{
    IUserManager _user = null;
    
    private UserAccessorFake _userAccessorFake;


    [TestInitialize]
    public void TestSetup()
    {
        _userAccessorFake = new UserAccessorFake();
        _user = new UserManager(_userAccessorFake);
    }

    // ========================================
    // EDIT USER TESTS - TDD RED-GREEN-REFACTOR
    // ========================================

    [TestMethod]
    public void TestEditUser_EditsUser()
    {
        // Arrange
        User oldUser = new User()
        {
            UserID = 1,
            TitleID = "Mr.",
            FirstName = "Test",
            LastName = "User1",
            Email = "testUser1@test.com",
            StatusID = "Veteran",
            AccountStatusID = "Active",
            MemLevelID = "Member"
        };
        User newUser = new User()
        {
            UserID = 1,
            TitleID = "Dr.",
            FirstName = "Updated",
            LastName = "User1Changed",
            Email = "updated@test.com",
            StatusID = "Active Duty",
            AccountStatusID = "Active",
            MemLevelID = "Admin"
        };
        bool actualResult = false;
        bool expectedResult = true;

        // Act
        actualResult = _user.EditUser(newUser, oldUser);

        // Assert
        Assert.AreEqual(expectedResult, actualResult);
    }

    [TestMethod]
    [ExpectedException(typeof(ApplicationException))]
    public void TestEditUser_FailsOnOldUserMismatch()
    {
        // Arrange - oldUser doesn't match any existing user
        User oldUser = new User()
        {
            UserID = 1,
            TitleID = "Mr.",
            FirstName = "Wrong",  // Wrong first name
            LastName = "User1",
            Email = "testUser1@test.com",
            StatusID = "Veteran",
            AccountStatusID = "Active",
            MemLevelID = "Member"
        };
        User newUser = new User()
        {
            UserID = 1,
            TitleID = "Dr.",
            FirstName = "Updated",
            LastName = "User1Changed",
            Email = "updated@test.com",
            StatusID = "Active Duty",
            AccountStatusID = "Active",
            MemLevelID = "Admin"
        };
        bool actualResult = false;

        // Act
        actualResult = _user.EditUser(newUser, oldUser);

        // Assert
        // Nothing to do - ExpectedException should be thrown
    }

    [TestMethod]
    public void TestEditUser_UpdatesEmailInCredentials()
    {
        // Arrange
        User oldUser = new User()
        {
            UserID = 1,
            TitleID = "Mr.",
            FirstName = "Test",
            LastName = "User1",
            Email = "testUser1@test.com",
            StatusID = "Veteran",
            AccountStatusID = "Active",
            MemLevelID = "Member"
        };
        User newUser = new User()
        {
            UserID = 1,
            TitleID = "Mr.",
            FirstName = "Test",
            LastName = "User1",
            Email = "newemail@test.com",  // Changed email
            StatusID = "Veteran",
            AccountStatusID = "Active",
            MemLevelID = "Member"
        };

        // Act
        bool editResult = _user.EditUser(newUser, oldUser);

        // Assert
        Assert.IsTrue(editResult, "Edit should succeed");

        // Verify the user can be found with new email
        User updatedUser = _user.GetUserByEmail("newemail@test.com");
        Assert.IsNotNull(updatedUser);
        Assert.AreEqual("newemail@test.com", updatedUser.Email);
    }

    [TestMethod]
    public void TestEditUser_UpdatesAllFields()
    {
        // Arrange
        User oldUser = new User()
        {
            UserID = 1,
            TitleID = "Mr.",
            FirstName = "Test",
            LastName = "User1",
            Email = "testUser1@test.com",
            StatusID = "Veteran",
            AccountStatusID = "Active",
            MemLevelID = "Member"
        };
        User newUser = new User()
        {
            UserID = 1,
            TitleID = "Dr.",
            FirstName = "Edward",
            LastName = "McKeown",
            Email = "edward@test.com",
            StatusID = "First Responder",
            AccountStatusID = "PendingVerify",
            MemLevelID = "Admin"
        };

        // Act
        bool editResult = _user.EditUser(newUser, oldUser);

        // Assert
        Assert.IsTrue(editResult, "Edit should succeed");

        // Verify all fields were updated
        User updatedUser = _user.GetUserByEmail("edward@test.com");
        Assert.AreEqual("Dr.", updatedUser.TitleID);
        Assert.AreEqual("Edward", updatedUser.FirstName);
        Assert.AreEqual("McKeown", updatedUser.LastName);
        Assert.AreEqual("First Responder", updatedUser.StatusID);
        Assert.AreEqual("PendingVerify", updatedUser.AccountStatusID);
        Assert.AreEqual("Admin", updatedUser.MemLevelID);
    }

    // ========================================
    // ADD USER TESTS - TDD RED-GREEN-REFACTOR
    // ========================================

    [TestMethod]
    public void TestAddUser_AddsNewUser()
    {
        // Arrange
        User newUser = new User()
        {
            TitleID = "Ms.",
            FirstName = "New",
            LastName = "User",
            Email = "newuser@test.com",
            StatusID = "Veteran",
            AccountStatusID = "Active",
            MemLevelID = "Member"
        };
        int expectedMinID = 100;  // Our fake starts at 100

        // Act
        int newUserID = _user.AddUser(newUser);

        // Assert
        Assert.IsTrue(newUserID >= expectedMinID, "New user ID should be generated");
    }

    [TestMethod]
    public void TestAddUser_ReturnsValidUserID()
    {
        // Arrange
        User newUser = new User()
        {
            TitleID = "Dr.",
            FirstName = "Doctor",
            LastName = "Who",
            Email = "doctor@tardis.com",
            StatusID = "Supporter",
            AccountStatusID = "Active",
            MemLevelID = "Guest"
        };

        // Act
        int newUserID = _user.AddUser(newUser);

        // Assert
        Assert.IsTrue(newUserID > 0, "User ID should be positive");

        // Verify user was actually added
        User addedUser = _user.GetUserByEmail("doctor@tardis.com");
        Assert.IsNotNull(addedUser);
        Assert.AreEqual(newUserID, addedUser.UserID);
    }

    [TestMethod]
    public void TestAddUser_SetsAllFieldsCorrectly()
    {
        // Arrange
        User newUser = new User()
        {
            TitleID = "Mrs.",
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane.doe@example.com",
            StatusID = "Spouse",
            AccountStatusID = "PendingVerify",
            MemLevelID = "Guest"
        };

        // Act
        int newUserID = _user.AddUser(newUser);

        // Assert
        User addedUser = _user.GetUserByEmail("jane.doe@example.com");
        Assert.IsNotNull(addedUser);
        Assert.AreEqual("Mrs.", addedUser.TitleID);
        Assert.AreEqual("Jane", addedUser.FirstName);
        Assert.AreEqual("Doe", addedUser.LastName);
        Assert.AreEqual("Spouse", addedUser.StatusID);
        Assert.AreEqual("PendingVerify", addedUser.AccountStatusID);
        Assert.AreEqual("Guest", addedUser.MemLevelID);
    }

    [TestMethod]
    [ExpectedException(typeof(ApplicationException))]
    public void TestAddUser_FailsOnDuplicateEmail()
    {
        // Arrange - try to add user with existing email
        User duplicateUser = new User()
        {
            TitleID = "Mr.",
            FirstName = "Duplicate",
            LastName = "User",
            Email = "testUser1@test.com",  // This email already exists
            StatusID = "Veteran",
            AccountStatusID = "Active",
            MemLevelID = "Member"
        };

        // Act
        int newUserID = _user.AddUser(duplicateUser);

        // Assert
        // Nothing to do - ExpectedException should be thrown
    }

    [TestMethod]
    public void TestAddUser_SetsDefaultActiveStatus()
    {
        // Arrange
        User newUser = new User()
        {
            TitleID = "Mx.",
            FirstName = "Active",
            LastName = "Test",
            Email = "active@test.com",
            StatusID = "Active Duty",
            AccountStatusID = "Active",
            MemLevelID = "Member"
        };

        // Act
        int newUserID = _user.AddUser(newUser);

        // Assert
        User addedUser = _user.GetUserByEmail("active@test.com");
        Assert.IsNotNull(addedUser);
        Assert.IsTrue(addedUser.Is_Active, "New user should be active by default");
        Assert.IsFalse(addedUser.AccountLocked, "New user account should not be locked");
    }

    [TestMethod]
    public void TestAddUser_GeneratesUniqueIDs()
    {
        // Arrange
        User user1 = new User()
        {
            TitleID = "Mr.",
            FirstName = "First",
            LastName = "User",
            Email = "first@unique.com",
            StatusID = "Veteran",
            AccountStatusID = "Active",
            MemLevelID = "Member"
        };
        User user2 = new User()
        {
            TitleID = "Ms.",
            FirstName = "Second",
            LastName = "User",
            Email = "second@unique.com",
            StatusID = "Veteran",
            AccountStatusID = "Active",
            MemLevelID = "Member"
        };

        // Act
        int userID1 = _user.AddUser(user1);
        int userID2 = _user.AddUser(user2);

        // Assert
        Assert.AreNotEqual(userID1, userID2, "User IDs should be unique");
    }

    // ========================================
    // EXISTING TESTS
    // ========================================


    [TestMethod]
    public void TestResetPasswordSucceedsWithCorrectEmailAndPassword()
    {
        // Arrange
        const string email = "testUser1@test.com";
        const string oldPassword = "newuser";
        const string newPassword = "Password!";
        const bool expectedResult = true;
        bool actualResult = false;

        // act
        actualResult = _user.ResetPassword(email, oldPassword, newPassword);

        // assert
        Assert.AreEqual(expectedResult, actualResult);
    }

    [TestMethod]
    [ExpectedException(typeof(ApplicationException))]
    public void TestResetPasswordFailsWithBadEmail()
    {
        // Arrange
        const string email = "testloser1@test.com";
        const string oldPassword = "newuser";
        const string newPassword = "Password!";
        bool actualResult = false;

        // act
        actualResult = _user.ResetPassword(email, oldPassword, newPassword);

        // Assert
        // Nothing

    }

    [TestMethod]
    [ExpectedException(typeof(ApplicationException))]
    public void TestResetPasswordFailsWithBadPassword()
    {
        // Arrange
        const string email = "testuser1@test.com";
        const string oldPassword = "newloser";
        const string newPassword = "Password!";
        bool actualResult = false;

        // act
        actualResult = _user.ResetPassword(email, oldPassword, newPassword);

        // Assert
        // Nothing

    }

    [TestMethod]
    public void TestAuthenticateUserPassesWithValidEmailAndPassword()
    {
        // arrange
        const string email = "testUser1@test.com";
        const string password = "newuser";
        const bool expectedResult = true;
        bool actualResult = false;

        // act
        actualResult = _user.AuthenticateUser(email, password);

        // assert
        Assert.AreEqual(expectedResult, actualResult);
    }

    [TestMethod]
    public void TestAuthenticateUserFailsForInactiveUser()
    {
        // arrange
        const string email = "testUser5@test.com";
        const string password = "newuser";
        const bool expectedResult = false;
        bool actualResult = true;

        // act
        actualResult = _user.AuthenticateUser(email, password);

        // assert
        Assert.AreEqual(expectedResult, actualResult);

    }

    [TestMethod]
    public void TestAuthenticateUserFailsWithBadEmail()
    {
        // arrange
        const string email = "testyuser@test.com";
        const string password = "newuser";
        const bool expectedResult = false;
        bool actualResult = true;

        // act
        actualResult = _user.AuthenticateUser(email, password);

        // assert
        Assert.AreEqual(expectedResult, actualResult);

    }

    [TestMethod]
    public void TestAuthenticateUserFailsWithBadPassword()
    {
        // arrange
        const string email = "testuser1@test.com";
        const string password = "noouser";
        const bool expectedResult = false;
        bool actualResult = true;

        // act
        actualResult = _user.AuthenticateUser(email, password);

        // assert
        Assert.AreEqual(expectedResult, actualResult);

    }

    [TestMethod]
    public void TestHashSha256ReturnsCorrectHashValue()
    {
        // Arrange
        const string password = "newuser";
        string expectedValue = "9c9064c59f1ffa2e174ee754d2979be80dd30db552ec03e7e327e9b1a4bd594e";
        string actualValue = null;

        // Act
        actualValue = _user.HashSha256(password);

        // Assert
        Assert.AreEqual(expectedValue, actualValue);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void TestHashSha256ThrowsNullArgumentExceptionForMissingInput()
    {
        // Arrange
        const string password = null;
        string actualValue = null;

        // Act
        actualValue = _user.HashSha256(password);

        // Assert
        // nothing to do - 
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void TestHashSha256ThrowsArgumentOutOfRangeExceptionForEmptyString()
    {
        // Arrange
        const string password = "";
        string actualValue = null;

        // Act
        actualValue = _user.HashSha256(password);

        // Assert
        // nothing to do - 
    }

    [TestMethod]
    public void TestGetUserByEmailReturnsCorrectUser()
    {
        // Arrange
        const string email = "testUser1@test.com";
        const int expectedUserID = 1;
        int actualUserID = 0;

        // Act
        actualUserID = (_user.GetUserByEmail(email)).UserID;

        // Assert
        Assert.AreEqual(expectedUserID, actualUserID);
    }

    [TestMethod]
    public void TestGetAccessForUserReturnsCorrectListForEmail()
    {
        // Arrange
        const string access1 = "Guest";
        const string access2 = "Member";
        const int accessCount = 2;
        const string email = "testUser1@test.com";
        List<string> actualAccess = null;

        // Act
        actualAccess = _user.GetAccessForUser(email);

        // Assert
        Assert.AreEqual(accessCount, actualAccess.Count);
        Assert.AreEqual(access1, actualAccess[0]);
        Assert.AreEqual(access2, actualAccess[1]);
    }

    [TestMethod]
    public void TestLoginUserReturnsCorrectUserVM()
    {
        // Arrange
        const string access1 = "Guest";
        const string access2 = "Member";
        const int accessCount = 2;
        const int userID = 1;
        const string email = "testUser1@test.com";
        const string password = "newuser";
        UserVM actualUserVM = null;

        // Act
        actualUserVM = _user.loginUser(email, password);

        // Assert
        Assert.AreEqual(accessCount, actualUserVM.Access.Count);
        Assert.AreEqual(access1, actualUserVM.Access[0]);
        Assert.AreEqual(access2, actualUserVM.Access[1]);
        Assert.AreEqual(userID, actualUserVM.UserID);


    }

    [TestMethod]
    public void GetUsersByActive_WithActiveStatus_ReturnsOnlyActiveUsers()
    {
        // Arrange
        _userAccessorFake.ClearFakeData();
        _userAccessorFake.AddFakeUser(new User
        {
            UserID = 100,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            AccountStatusID = "Active",
            Is_Active = true
        });

        _userAccessorFake.AddFakeUser(new User
        {
            UserID = 101,
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane@example.com",
            AccountStatusID = "Inactive",
            Is_Active = false
        });

        // Act
        var result = _user.GetUsersByActive("Active");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count, "Should return only active users");
        Assert.AreEqual("Active", result[0].AccountStatusID);
        Assert.AreEqual("John", result[0].FirstName);
    }

    [TestMethod]
    public void GetUsersByActive_WithPendingVerify_ReturnsOnlyPendingUsers()
    {
        // Arrange
        _userAccessorFake.ClearFakeData();
        _userAccessorFake.AddFakeUser(new User
        {
            UserID = 100,
            Email = "active@example.com",
            AccountStatusID = "Active"
        });

        _userAccessorFake.AddFakeUser(new User
        {
            UserID = 101,
            Email = "pending@example.com",
            AccountStatusID = "PendingVerify"
        });

        _userAccessorFake.AddFakeUser(new User
        {
            UserID = 102,
            Email = "pending2@example.com",
            AccountStatusID = "PendingVerify"
        });

        // Act
        var result = _user.GetUsersByActive("PendingVerify");

        // Assert
        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result.TrueForAll(u => u.AccountStatusID == "PendingVerify"));
    }

    [TestMethod]
    public void GetUsersByActive_WithNoMatchingStatus_ReturnsEmptyList()
    {
        // Arrange
        _userAccessorFake.ClearFakeData();
        _userAccessorFake.AddFakeUser(new User
        {
            UserID = 100,
            Email = "user@example.com",
            AccountStatusID = "Active"
        });

        // Act
        var result = _user.GetUsersByActive("Suspended");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void HashSha256_WithSameInput_ProducesSameHash()
    {
        // Arrange
        string password = "TestPassword123";

        // Act
        string hash1 = _user.HashSha256(password);
        string hash2 = _user.HashSha256(password);

        // Assert
        Assert.AreEqual(hash1, hash2, "Same input should produce same hash");
        Assert.AreEqual(64, hash1.Length, "SHA256 hash should be 64 characters");
    }

    [TestMethod]
    public void HashSha256_WithDifferentInputs_ProducesDifferentHashes()
    {
        // Arrange
        string password1 = "Password123";
        string password2 = "Password456";

        // Act
        string hash1 = _user.HashSha256(password1);
        string hash2 = _user.HashSha256(password2);

        // Assert
        Assert.AreNotEqual(hash1, hash2, "Different passwords should produce different hashes");
    }

    [TestMethod]
    public void HashSha256_WithEmptyString_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        string emptyPassword = "";

        // Act & Assert
        Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
        {
            _user.HashSha256(emptyPassword);
        });
    }

    [TestMethod]
    public void GetUserByEmail_WithValidEmail_ReturnsCorrectUser()
    {
        // Arrange
        _userAccessorFake.AddFakeUser(new User
        {
            UserID = 999,
            FirstName = "Edward",
            LastName = "McKeown",
            Email = "edward@doctormckeown.com",
            StatusID = "Veteran",
            AccountStatusID = "Active",
            MemLevelID = "Admin"
        });

        // Act
        var result = _user.GetUserByEmail("edward@doctormckeown.com");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Edward", result.FirstName);
        Assert.AreEqual("McKeown", result.LastName);
        Assert.AreEqual("Admin", result.MemLevelID);
    }

    [TestMethod]
    public void GetUserByEmail_WithNonExistentEmail_ThrowsApplicationException()
    {
        // Arrange
        string nonExistentEmail = "nonexistent@example.com";

        // Act & Assert
        var exception = Assert.ThrowsException<ApplicationException>(() =>
        {
            _user.GetUserByEmail(nonExistentEmail);
        });

        Assert.IsTrue(exception.Message.Contains("user not found"));
    }

    [TestMethod]
    public void AuthenticateUser_WithCorrectCredentials_ReturnsTrue()
    {
        // Arrange
        string email = "testUser1@test.com";
        string password = "newuser";

        // Act
        bool result = _user.AuthenticateUser(email, password);

        // Assert
        Assert.IsTrue(result, "Authentication should succeed with correct credentials");
    }

    [TestMethod]
    public void AuthenticateUser_WithIncorrectPassword_ReturnsFalse()
    {
        // Arrange
        string email = "testUser1@test.com";
        string wrongPassword = "wrongpassword";

        // Act
        bool result = _user.AuthenticateUser(email, wrongPassword);

        // Assert
        Assert.IsFalse(result, "Authentication should fail with incorrect password");
    }

    [TestMethod]
    public void LoginUser_WithLockedAccount_ThrowsApplicationException()
    {
        // Arrange
        string email = "locked@example.com";
        string password = "password123";

        _userAccessorFake.AddFakeUser(new User
        {
            UserID = 900,
            Email = email,
            FirstName = "Locked",
            LastName = "User",
            AccountLocked = true,
            AccountStatusID = "Active",
            MemLevelID = "Member",
            Is_Active = true
        });

        // Act & Assert
        var exception = Assert.ThrowsException<ApplicationException>(() =>
        {
            _user.loginUser(email, password);
        });

        Assert.IsTrue(exception.Message.Contains("locked"),
            "Exception should mention account is locked");
    }

    [TestMethod]
    public void LoginUser_WithValidCredentials_ReturnsUserVM()
    {
        // Arrange
        string email = "valid@example.com";
        string password = "password123";
        string hashedPassword = _user.HashSha256(password);

        _userAccessorFake.AddFakeUser(new User
        {
            UserID = 800,
            TitleID = "Mr.",
            FirstName = "Valid",
            LastName = "User",
            Email = email,
            StatusID = "Veteran",
            AccountStatusID = "Active",
            MemLevelID = "Member",
            AccountLocked = false,
            Is_Active = true,
            RegistrationDate = DateTime.Now
        });

        _userAccessorFake.AddFakeCredential(email, hashedPassword);
        _userAccessorFake.AddFakeAccess(email, "Member");

        // Act
        var result = _user.loginUser(email, password);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Valid", result.FirstName);
        Assert.AreEqual("User", result.LastName);
        Assert.IsNotNull(result.Access);
        Assert.AreEqual(1, result.Access.Count);
        Assert.AreEqual("Member", result.Access[0]);
        Assert.AreEqual(800, result.UserID);
    }

    [TestMethod]
    public void ResetPassword_WithCorrectOldPassword_ReturnsTrue()
    {
        // Arrange
        string email = "resettest@example.com";
        string oldPassword = "oldpass123";
        string newPassword = "newpass456";

        string hashedOldPassword = _user.HashSha256(oldPassword);

        _userAccessorFake.AddFakeUser(new User
        {
            UserID = 700,
            Email = email,
            Is_Active = true
        });
        _userAccessorFake.AddFakeCredential(email, hashedOldPassword);

        // Act
        bool result = _user.ResetPassword(email, oldPassword, newPassword);

        // Assert
        Assert.IsTrue(result, "Password reset should succeed");
    }

    [TestMethod]
    public void ResetPassword_WithIncorrectOldPassword_ThrowsApplicationException()
    {
        // Arrange
        string email = "resetfail@example.com";
        string oldPassword = "oldpass123";
        string wrongOldPassword = "wrongpass";
        string newPassword = "newpass456";

        string hashedOldPassword = _user.HashSha256(oldPassword);

        _userAccessorFake.AddFakeUser(new User
        {
            UserID = 701,
            Email = email,
            Is_Active = true
        });
        _userAccessorFake.AddFakeCredential(email, hashedOldPassword);

        // Act & Assert
        Assert.ThrowsException<ApplicationException>(() =>
        {
            _user.ResetPassword(email, wrongOldPassword, newPassword);
        });
    }


}


















