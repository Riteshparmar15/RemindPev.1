using RemindPe.Models;

namespace RemindPe.Data;

public static class DbInitializer
{
    public static void Seed(KhataDbContext context)
    {
        if (!context.Customers.Any())
        {
            SeedDemoData(context, false);
        }
    }

    public static void SeedDemoData(KhataDbContext context, bool clearExisting = false)
    {
        if (clearExisting)
        {
            context.Transactions.RemoveRange(context.Transactions);
            context.Customers.RemoveRange(context.Customers);
            context.SaveChanges();
        }

        var now = DateTime.UtcNow;

        var customers = new List<Customer>
        {
            new Customer
            {
                Name = "Rajesh Sharma (Kirana & General)",
                Phone = "9876543210",
                Address = "Shop 4, Market Yard, Civil Lines",
                CreatedAt = now.AddDays(-30),
                Transactions = new List<Transaction>
                {
                    new Transaction
                    {
                        Amount = 3200m,
                        Type = "Credit",
                        Note = "Monthly grocery ration & edible oil 5L",
                        Date = now.AddDays(-20),
                        CreatedAt = now.AddDays(-20)
                    },
                    new Transaction
                    {
                        Amount = 1500m,
                        Type = "Debit",
                        Note = "Google Pay UPI settlement (Ref: GP4092)",
                        Date = now.AddDays(-12),
                        CreatedAt = now.AddDays(-12)
                    },
                    new Transaction
                    {
                        Amount = 1750m,
                        Type = "Credit",
                        Note = "Wheat flour 30kg + Basmati rice 10kg",
                        Date = now.AddDays(-5),
                        CreatedAt = now.AddDays(-5)
                    }
                }
            },
            new Customer
            {
                Name = "Priya Patel (Patel Family)",
                Phone = "9820112345",
                Address = "B-12, Shanti Kunj, Maninagar",
                CreatedAt = now.AddDays(-25),
                Transactions = new List<Transaction>
                {
                    new Transaction
                    {
                        Amount = 2400m,
                        Type = "Credit",
                        Note = "Festival dry fruits & sweet hampers",
                        Date = now.AddDays(-18),
                        CreatedAt = now.AddDays(-18)
                    },
                    new Transaction
                    {
                        Amount = 1200m,
                        Type = "Debit",
                        Note = "Cash payment at store counter",
                        Date = now.AddDays(-9),
                        CreatedAt = now.AddDays(-9)
                    },
                    new Transaction
                    {
                        Amount = 650m,
                        Type = "Credit",
                        Note = "Spices & packaging supplies",
                        Date = now.AddDays(-2),
                        CreatedAt = now.AddDays(-2)
                    }
                }
            },
            new Customer
            {
                Name = "Amit Verma (Verma Electricals)",
                Phone = "9313512434",
                Address = "Main Commercial Complex, Laxmi Nagar",
                CreatedAt = now.AddDays(-28),
                Transactions = new List<Transaction>
                {
                    new Transaction
                    {
                        Amount = 6800m,
                        Type = "Credit",
                        Note = "Modular switches, copper wire bundles & LED panels",
                        Date = now.AddDays(-24),
                        CreatedAt = now.AddDays(-24)
                    },
                    new Transaction
                    {
                        Amount = 3000m,
                        Type = "Debit",
                        Note = "PhonePe transfer (UTR: 9812401)",
                        Date = now.AddDays(-14),
                        CreatedAt = now.AddDays(-14)
                    },
                    new Transaction
                    {
                        Amount = 1800m,
                        Type = "Credit",
                        Note = "Submersible cable & circuit breakers",
                        Date = now.AddDays(-4),
                        CreatedAt = now.AddDays(-4)
                    }
                }
            },
            new Customer
            {
                Name = "Sunita Devi (Home Baker)",
                Phone = "9711223344",
                Address = "Flat 304, Green Valley, Sector 21",
                CreatedAt = now.AddDays(-15),
                Transactions = new List<Transaction>
                {
                    new Transaction
                    {
                        Amount = 1850m,
                        Type = "Credit",
                        Note = "Baking chocolate, cocoa butter & vanilla extract",
                        Date = now.AddDays(-10),
                        CreatedAt = now.AddDays(-10)
                    },
                    new Transaction
                    {
                        Amount = 1850m,
                        Type = "Debit",
                        Note = "Paytm UPI full clearance",
                        Date = now.AddDays(-2),
                        CreatedAt = now.AddDays(-2)
                    }
                }
            },
            new Customer
            {
                Name = "Vikram Malhotra (Malhotra Sweets)",
                Phone = "9811223344",
                Address = "Near Clock Tower, Mall Road",
                CreatedAt = now.AddDays(-45),
                Transactions = new List<Transaction>
                {
                    new Transaction
                    {
                        Amount = 8500m,
                        Type = "Credit",
                        Note = "Pure desi ghee 15kg tins & organic sugar",
                        Date = now.AddDays(-34),
                        CreatedAt = now.AddDays(-34)
                    },
                    new Transaction
                    {
                        Amount = 6400m,
                        Type = "Credit",
                        Note = "Cardamom, saffron & cashew nuts 5kg",
                        Date = now.AddDays(-21),
                        CreatedAt = now.AddDays(-21)
                    },
                    new Transaction
                    {
                        Amount = 2500m,
                        Type = "Debit",
                        Note = "Bank IMPS transfer",
                        Date = now.AddDays(-15),
                        CreatedAt = now.AddDays(-15)
                    }
                }
            },
            new Customer
            {
                Name = "Mohammed Farhan (Farhan Hardware)",
                Phone = "9845123456",
                Address = "12/A, Broadway Road, Commercial Street",
                CreatedAt = now.AddDays(-35),
                Transactions = new List<Transaction>
                {
                    new Transaction
                    {
                        Amount = 9200m,
                        Type = "Credit",
                        Note = "PVC drainage pipes & brass ball valves",
                        Date = now.AddDays(-28),
                        CreatedAt = now.AddDays(-28)
                    },
                    new Transaction
                    {
                        Amount = 4000m,
                        Type = "Debit",
                        Note = "Cash jama at counter",
                        Date = now.AddDays(-16),
                        CreatedAt = now.AddDays(-16)
                    },
                    new Transaction
                    {
                        Amount = 2050m,
                        Type = "Credit",
                        Note = "Drill bits & waterproof cement primer",
                        Date = now.AddDays(-6),
                        CreatedAt = now.AddDays(-6)
                    }
                }
            },
            new Customer
            {
                Name = "Sneha Gupta (Gupta Garments)",
                Phone = "9933445566",
                Address = "Shop 18, Metro Plaza, Park Street",
                CreatedAt = now.AddDays(-20),
                Transactions = new List<Transaction>
                {
                    new Transaction
                    {
                        Amount = 1500m,
                        Type = "Credit",
                        Note = "Packaging polythene rolls & tags",
                        Date = now.AddDays(-15),
                        CreatedAt = now.AddDays(-15)
                    },
                    new Transaction
                    {
                        Amount = 4000m,
                        Type = "Debit",
                        Note = "Advance token deposit for bulk Diwali bags",
                        Date = now.AddDays(-3),
                        CreatedAt = now.AddDays(-3)
                    }
                }
            },
            new Customer
            {
                Name = "Suresh Yadav (Yadav Dairy Farm)",
                Phone = "9823456789",
                Address = "Survey 44, Kothrud Bypass",
                CreatedAt = now.AddDays(-22),
                Transactions = new List<Transaction>
                {
                    new Transaction
                    {
                        Amount = 5800m,
                        Type = "Credit",
                        Note = "Cattle feed sacks & calcium supplements",
                        Date = now.AddDays(-19),
                        CreatedAt = now.AddDays(-19)
                    },
                    new Transaction
                    {
                        Amount = 2500m,
                        Type = "Debit",
                        Note = "GPay payment received",
                        Date = now.AddDays(-11),
                        CreatedAt = now.AddDays(-11)
                    },
                    new Transaction
                    {
                        Amount = 800m,
                        Type = "Credit",
                        Note = "Milking machine sanitizing liquid",
                        Date = now.AddDays(-3),
                        CreatedAt = now.AddDays(-3)
                    }
                }
            },
            new Customer
            {
                Name = "Ananya Iyer (Iyer Tiffin & Catering)",
                Phone = "9840123456",
                Address = "North Mada Street, Mylapore",
                CreatedAt = now.AddDays(-18),
                Transactions = new List<Transaction>
                {
                    new Transaction
                    {
                        Amount = 3600m,
                        Type = "Credit",
                        Note = "Ponni boiled rice 50kg + Sambar dal",
                        Date = now.AddDays(-14),
                        CreatedAt = now.AddDays(-14)
                    },
                    new Transaction
                    {
                        Amount = 3600m,
                        Type = "Debit",
                        Note = "UPI QR Instant Settlement",
                        Date = now.AddDays(-1),
                        CreatedAt = now.AddDays(-1)
                    }
                }
            },
            new Customer
            {
                Name = "Deepak Chauhan (Chauhan Auto Works)",
                Phone = "9899012345",
                Address = "Plot 5, Industrial Area, GT Road",
                CreatedAt = now.AddDays(-32),
                Transactions = new List<Transaction>
                {
                    new Transaction
                    {
                        Amount = 11400m,
                        Type = "Credit",
                        Note = "Engine synthetic oil drums & hydraulic fluids",
                        Date = now.AddDays(-26),
                        CreatedAt = now.AddDays(-26)
                    },
                    new Transaction
                    {
                        Amount = 5000m,
                        Type = "Debit",
                        Note = "NEFT transfer from PNB",
                        Date = now.AddDays(-13),
                        CreatedAt = now.AddDays(-13)
                    },
                    new Transaction
                    {
                        Amount = 2500m,
                        Type = "Credit",
                        Note = "Oil filter cartridges & spark plugs",
                        Date = now.AddDays(-4),
                        CreatedAt = now.AddDays(-4)
                    }
                }
            },
            new Customer
            {
                Name = "Kavita Reddy (Reddy Enterprises)",
                Phone = "9849012345",
                Address = "Road No. 10, Banjara Hills",
                CreatedAt = now.AddDays(-24),
                Transactions = new List<Transaction>
                {
                    new Transaction
                    {
                        Amount = 4250m,
                        Type = "Credit",
                        Note = "Copier paper cartons & stationery desk sets",
                        Date = now.AddDays(-16),
                        CreatedAt = now.AddDays(-16)
                    },
                    new Transaction
                    {
                        Amount = 2500m,
                        Type = "Debit",
                        Note = "Cash received by store delivery boy",
                        Date = now.AddDays(-8),
                        CreatedAt = now.AddDays(-8)
                    },
                    new Transaction
                    {
                        Amount = 1000m,
                        Type = "Credit",
                        Note = "Highlighters & spiral notebooks",
                        Date = now.AddDays(-2),
                        CreatedAt = now.AddDays(-2)
                    }
                }
            },
            new Customer
            {
                Name = "Harpreet Singh (Singh Dhaba)",
                Phone = "9872012345",
                Address = "Highway Milestone 12, Sector 17",
                CreatedAt = now.AddDays(-48),
                Transactions = new List<Transaction>
                {
                    new Transaction
                    {
                        Amount = 12000m,
                        Type = "Credit",
                        Note = "Basmati rice 100kg + Mustard oil cans",
                        Date = now.AddDays(-38),
                        CreatedAt = now.AddDays(-38)
                    },
                    new Transaction
                    {
                        Amount = 7800m,
                        Type = "Credit",
                        Note = "Fresh cottage cheese paneer crates",
                        Date = now.AddDays(-22),
                        CreatedAt = now.AddDays(-22)
                    },
                    new Transaction
                    {
                        Amount = 5000m,
                        Type = "Debit",
                        Note = "Cheque clearance HDFC Bank",
                        Date = now.AddDays(-14),
                        CreatedAt = now.AddDays(-14)
                    }
                }
            },
            new Customer
            {
                Name = "Pooja Mehra (Mehra Boutique)",
                Phone = "9810123456",
                Address = "Main Market, Rajouri Garden",
                CreatedAt = now.AddDays(-16),
                Transactions = new List<Transaction>
                {
                    new Transaction
                    {
                        Amount = 2100m,
                        Type = "Credit",
                        Note = "Embroidery threads & designer lace",
                        Date = now.AddDays(-11),
                        CreatedAt = now.AddDays(-11)
                    },
                    new Transaction
                    {
                        Amount = 2100m,
                        Type = "Debit",
                        Note = "Google Pay clearance",
                        Date = now.AddDays(-2),
                        CreatedAt = now.AddDays(-2)
                    }
                }
            },
            new Customer
            {
                Name = "Ramesh Goyal (Goyal Provisions)",
                Phone = "9837012345",
                Address = "Grain Market, Sadar Bazar",
                CreatedAt = now.AddDays(-26),
                Transactions = new List<Transaction>
                {
                    new Transaction
                    {
                        Amount = 8500m,
                        Type = "Credit",
                        Note = "Refined sugar sacks & toor dal 50kg",
                        Date = now.AddDays(-17),
                        CreatedAt = now.AddDays(-17)
                    },
                    new Transaction
                    {
                        Amount = 3200m,
                        Type = "Debit",
                        Note = "Counter cash collection",
                        Date = now.AddDays(-7),
                        CreatedAt = now.AddDays(-7)
                    },
                    new Transaction
                    {
                        Amount = 1000m,
                        Type = "Credit",
                        Note = "Mustard seeds & whole cumin",
                        Date = now.AddDays(-1),
                        CreatedAt = now.AddDays(-1)
                    }
                }
            },
            new Customer
            {
                Name = "Manish Joshi (Joshi Medical)",
                Phone = "9425012345",
                Address = "Zone II, MP Nagar",
                CreatedAt = now.AddDays(-18),
                Transactions = new List<Transaction>
                {
                    new Transaction
                    {
                        Amount = 2800m,
                        Type = "Credit",
                        Note = "Surgical cotton & antiseptic gallon cans",
                        Date = now.AddDays(-12),
                        CreatedAt = now.AddDays(-12)
                    },
                    new Transaction
                    {
                        Amount = 4000m,
                        Type = "Debit",
                        Note = "Advance payment for pulse oximeters",
                        Date = now.AddDays(-4),
                        CreatedAt = now.AddDays(-4)
                    }
                }
            },
            new Customer
            {
                Name = "Meenakshi Sundaram (Sundaram Handlooms)",
                Phone = "9444012345",
                Address = "Cross Cut Road, Gandhipuram",
                CreatedAt = now.AddDays(-36),
                Transactions = new List<Transaction>
                {
                    new Transaction
                    {
                        Amount = 13500m,
                        Type = "Credit",
                        Note = "Pure cotton weaving yarn & natural dyes",
                        Date = now.AddDays(-29),
                        CreatedAt = now.AddDays(-29)
                    },
                    new Transaction
                    {
                        Amount = 6000m,
                        Type = "Debit",
                        Note = "UPI payment via ICICI",
                        Date = now.AddDays(-10),
                        CreatedAt = now.AddDays(-10)
                    },
                    new Transaction
                    {
                        Amount = 2000m,
                        Type = "Credit",
                        Note = "Finishing starch & loom accessories",
                        Date = now.AddDays(-3),
                        CreatedAt = now.AddDays(-3)
                    }
                }
            },
            new Customer
            {
                Name = "Arjun Das (Das Tea & Cafe)",
                Phone = "9864012345",
                Address = "Station Road, Paltan Bazar",
                CreatedAt = now.AddDays(-20),
                Transactions = new List<Transaction>
                {
                    new Transaction
                    {
                        Amount = 4500m,
                        Type = "Credit",
                        Note = "CTC Assam tea dust 20kg + Sugar bags",
                        Date = now.AddDays(-15),
                        CreatedAt = now.AddDays(-15)
                    },
                    new Transaction
                    {
                        Amount = 2000m,
                        Type = "Debit",
                        Note = "Daily cash collection",
                        Date = now.AddDays(-6),
                        CreatedAt = now.AddDays(-6)
                    },
                    new Transaction
                    {
                        Amount = 700m,
                        Type = "Credit",
                        Note = "Dairy milk cartons & rusk boxes",
                        Date = now,
                        CreatedAt = now
                    }
                }
            },
            new Customer
            {
                Name = "Bhavna Shah (Shah Jewellers Staff)",
                Phone = "9825012345",
                Address = "Silver Arc Plaza, CG Road",
                CreatedAt = now.AddDays(-14),
                Transactions = new List<Transaction>
                {
                    new Transaction
                    {
                        Amount = 3000m,
                        Type = "Credit",
                        Note = "Velvet presentation boxes & jewelry pouch bags",
                        Date = now.AddDays(-9),
                        CreatedAt = now.AddDays(-9)
                    },
                    new Transaction
                    {
                        Amount = 3000m,
                        Type = "Debit",
                        Note = "Online payment settlement",
                        Date = now.AddDays(-3),
                        CreatedAt = now.AddDays(-3)
                    }
                }
            }
        };

        context.Customers.AddRange(customers);

        // Update settings
        var settings = context.Settings.FirstOrDefault();
        if (settings == null)
        {
            context.Settings.Add(new MerchantSettings
            {
                StoreName = "Parmar Kirana & Vyapar Mart",
                OwnerName = "Ritesh Parmar",
                Phone = "9313512434",
                UpiId = "riteshparmar15@oksbi",
                Currency = "₹",
                DefaultReminderTemplate = "hinglish",
                ReceiptFooter = "Thank you for shopping with us! For questions contact +91 93135 12434",
                UpdatedAt = now
            });
        }
        else
        {
            settings.StoreName = "Parmar Kirana & Vyapar Mart";
            settings.OwnerName = "Ritesh Parmar";
            settings.Phone = "9313512434";
            settings.UpiId = "riteshparmar15@oksbi";
            settings.Currency = "₹";
            settings.DefaultReminderTemplate = "hinglish";
            settings.ReceiptFooter = "Thank you for shopping with us! For questions contact +91 93135 12434";
            settings.UpdatedAt = now;
        }

        context.SaveChanges();
    }
}
