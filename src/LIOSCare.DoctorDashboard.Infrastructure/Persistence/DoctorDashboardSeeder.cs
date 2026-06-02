using LIOSCare.DoctorDashboard.Domain.Entities;
using LIOSCare.DoctorDashboard.Domain.Enums;
using LIOSCare.DoctorDashboard.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LIOSCare.DoctorDashboard.Infrastructure.Persistence;

public sealed class DoctorDashboardSeeder(DoctorPortalDbContext db, PasswordHashingService hasher, ILogger<DoctorDashboardSeeder> logger)
{
    private static readonly Guid DoctorId       = Guid.Parse("30000000-0000-0000-0000-000000000001");
    private static readonly Guid AccountId      = Guid.Parse("20000000-0000-0000-0000-000000000001");
    private static readonly Guid TierDiamond    = Guid.Parse("10000000-0000-0000-0000-000000000001");
    private static readonly Guid TierElite      = Guid.Parse("10000000-0000-0000-0000-000000000002");
    private static readonly Guid TierStandard   = Guid.Parse("10000000-0000-0000-0000-000000000003");
    private static readonly Guid NoahUserId     = Guid.Parse("40000000-0000-0000-0000-000000000001");
    private static readonly Guid NoahBookingId  = Guid.Parse("50000000-0000-0000-0000-000000000001");

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await SeedServiceTiersAsync(ct);
        await SeedDoctorAsync(ct);
        await db.SaveChangesAsync(ct);
        await SeedQuickChatsAsync(ct);
        await SeedBookingsAsync(ct);
        await db.SaveChangesAsync(ct);
        await SeedSessionReportsAsync(ct);
        await db.SaveChangesAsync(ct);
    }

    // ── Service Tiers ────────────────────────────────────────────────────────

    private async Task SeedServiceTiersAsync(CancellationToken ct)
    {
        if (await db.ServiceTiers.AnyAsync(ct)) return;
        db.ServiceTiers.AddRange(
            new ServiceTier
            {
                Id = TierDiamond, Name = "Diamond", PriceUsd = 99.00m,
                ResponseWindowHours = 24, IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow,
                Features = new[] { "30-minute video consultation", "Priority quick-chat response", "Session report summary" }
            },
            new ServiceTier
            {
                Id = TierElite, Name = "Elite", PriceUsd = 25.00m,
                ResponseWindowHours = 24, IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow,
                Features = new[] { "Text consultation", "One detailed response", "Follow-up guidance" }
            },
            new ServiceTier
            {
                Id = TierStandard, Name = "Standard", PriceUsd = 10.00m,
                ResponseWindowHours = 72, IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow,
                Features = new[] { "One-time reply", "72-hour response window", "Basic guidance" }
            }
        );
    }

    // ── Demo Doctor Account ──────────────────────────────────────────────────
    //  Login: doctor@lioscare.local / Doctor@123

    private async Task SeedDoctorAsync(CancellationToken ct)
    {
        const string demoEmail    = "doctor@lioscare.local";
        const string demoPassword = "Doctor@123";

        // ── Account: create or repair ────────────────────────────────────────
        var account = await db.DoctorAccounts
            .FirstOrDefaultAsync(x => x.Email == demoEmail, ct);

        if (account is null)
        {
            account = new DoctorAccount
            {
                Id             = AccountId,
                Email          = demoEmail,
                PasswordHash   = hasher.Hash(demoPassword),
                EmailConfirmed = true,
                IsActive       = true,
                CreatedAt      = DateTimeOffset.UtcNow.AddMonths(-10)
            };
            db.DoctorAccounts.Add(account);
            logger.LogInformation("[Seed] Demo doctor account created: {Email}", demoEmail);
        }
        else
        {
            account.PasswordHash   = hasher.Hash(demoPassword);
            account.IsActive       = true;
            account.EmailConfirmed = true;
            logger.LogInformation("[Seed] Demo doctor account repaired: {Email}", demoEmail);
        }

        // ── Profile: create if missing (never duplicate) ─────────────────────
        var profileExists = await db.DoctorProfiles
            .AnyAsync(x => x.DoctorAccountId == account.Id, ct);

        if (!profileExists)
        {
            db.DoctorProfiles.Add(new DoctorProfile
            {
                Id               = DoctorId,
                DoctorAccountId  = account.Id,
                FullName         = "Dr. Julia Adams",
                Specializations  = new[] { "Depression", "Trauma", "Anxiety" },
                YearsExperience  = 10,
                Rating           = 4.8m,
                PatientCount     = 250,
                Bio              = "Licensed psychology professional focused on emotional resilience, trauma recovery, and practical therapy planning.",
                Certifications   = new[] { "CBT Certified", "Trauma-Informed Care", "Mindfulness-Based Therapy" },
                ProfilePhotoUrl  = "/img/doctor-avatar.svg",
                IsAvailable      = true,
                CreatedAt        = DateTimeOffset.UtcNow.AddMonths(-10),
                UpdatedAt        = DateTimeOffset.UtcNow
            });
            logger.LogInformation("[Seed] Demo doctor profile created for account {AccountId}", account.Id);
        }

        // ── Education: create if no rows exist for this doctor ───────────────
        if (!await db.DoctorEducations.AnyAsync(x => x.DoctorId == DoctorId, ct))
        {
            db.DoctorEducations.AddRange(
                new DoctorEducation { Id = Guid.NewGuid(), DoctorId = DoctorId, Degree = "Ph.D. in Clinical Psychology", Institution = "Stanford University", Year = 2014 },
                new DoctorEducation { Id = Guid.NewGuid(), DoctorId = DoctorId, Degree = "M.Sc. Psychology",            Institution = "Columbia University",  Year = 2009 }
            );
        }
    }

    // ── Quick Chat Requests ──────────────────────────────────────────────────
    //  Variety: 1 healthy SLA, 1 warning, 1 overdue, 1 danger, 1 already replied (history)

    private async Task SeedQuickChatsAsync(CancellationToken ct)
    {
        if (await db.QuickChatRequests.AnyAsync(ct)) return;
        var now = DateTimeOffset.UtcNow;
        db.QuickChatRequests.AddRange(
            // Directed at doctor — SLA healthy (green)
            new QuickChatRequest
            {
                Id = Guid.NewGuid(), UserId = Guid.NewGuid(),
                UserFirstName = "Sarah", UserLastInitial = "M",
                TierId = TierDiamond, DoctorId = DoctorId,
                Status = QuickChatStatus.Pending,
                UserMessage = "I have been having panic symptoms before work every morning. Could you suggest a first practical step?",
                CreatedAt = now.AddHours(-3), SlaDeadline = now.AddHours(21)
            },
            // General pool — SLA warning (< 6 h, amber)
            new QuickChatRequest
            {
                Id = Guid.NewGuid(), UserId = Guid.NewGuid(),
                UserFirstName = "Liam", UserLastInitial = "J",
                TierId = TierElite, DoctorId = null,
                Status = QuickChatStatus.Pending,
                UserMessage = "I need help calming down after a heated argument with a family member. I cannot sleep.",
                CreatedAt = now.AddHours(-19), SlaDeadline = now.AddHours(5)
            },
            // General pool — SLA danger (< 2 h, red)
            new QuickChatRequest
            {
                Id = Guid.NewGuid(), UserId = Guid.NewGuid(),
                UserFirstName = "Mia", UserLastInitial = "B",
                TierId = TierStandard, DoctorId = null,
                Status = QuickChatStatus.Pending,
                UserMessage = "I feel completely stuck and unmotivated. What can I try today to break the cycle?",
                CreatedAt = now.AddHours(-71), SlaDeadline = now.AddHours(1)
            },
            // General pool — SLA overdue
            new QuickChatRequest
            {
                Id = Guid.NewGuid(), UserId = Guid.NewGuid(),
                UserFirstName = "Omar", UserLastInitial = "K",
                TierId = TierElite, DoctorId = null,
                Status = QuickChatStatus.Pending,
                UserMessage = "I have been struggling with intrusive thoughts at night and it is affecting my work performance.",
                CreatedAt = now.AddHours(-30), SlaDeadline = now.AddMinutes(-45)
            },
            // Already replied — visible in history
            new QuickChatRequest
            {
                Id = Guid.NewGuid(), UserId = Guid.NewGuid(),
                UserFirstName = "Priya", UserLastInitial = "S",
                TierId = TierDiamond, DoctorId = DoctorId,
                Status = QuickChatStatus.Replied,
                UserMessage = "I feel disconnected from daily life. Is this dissociation? What should I do?",
                DoctorReply = "What you are describing sounds like it could be a mild dissociative response to stress. This is more common than people realize. The most important first step is grounding — try the 5-4-3-2-1 technique: name 5 things you can see, 4 you can touch, 3 you can hear, 2 you can smell, 1 you can taste. Do this slowly. I recommend booking a full session so we can explore this properly.",
                CreatedAt = now.AddDays(-2), SlaDeadline = now.AddDays(-2).AddHours(24),
                AcceptedAt = now.AddDays(-2).AddHours(1), RepliedAt = now.AddDays(-2).AddHours(3)
            }
        );
    }

    // ── Direct Booking Requests ──────────────────────────────────────────────
    //  Variety: pending Diamond, pending Elite, accepted Elite, declined Standard

    private async Task SeedBookingsAsync(CancellationToken ct)
    {
        if (await db.DirectBookingRequests.AnyAsync(ct)) return;
        var now = DateTimeOffset.UtcNow;

        db.DirectBookingRequests.AddRange(
            // Pending — Diamond (shows "schedule required" badge + accept/decline UI)
            new DirectBookingRequest
            {
                Id = Guid.NewGuid(), UserId = Guid.NewGuid(),
                UserFullName = "Lindsay Stuart", DoctorId = DoctorId,
                TierId = TierDiamond, Status = BookingStatus.Pending,
                PaymentStatus = PaymentStatus.Paid,
                AmountUsd = 99.00m, PlatformFeeUsd = 4.95m,
                Notes = "I would like to discuss recurring anxiety that spikes before important presentations at work.",
                CreatedAt = now.AddHours(-4)
            },
            // Pending — Elite
            new DirectBookingRequest
            {
                Id = Guid.NewGuid(), UserId = Guid.NewGuid(),
                UserFullName = "Amara Osei", DoctorId = DoctorId,
                TierId = TierElite, Status = BookingStatus.Pending,
                PaymentStatus = PaymentStatus.Paid,
                AmountUsd = 25.00m, PlatformFeeUsd = 1.25m,
                Notes = "Struggling with grief after losing a close friend. Need guidance on where to start.",
                CreatedAt = now.AddHours(-1)
            },
            // Accepted — Elite (linked to session reports)
            new DirectBookingRequest
            {
                Id = NoahBookingId, UserId = NoahUserId,
                UserFullName = "Noah Peterson", DoctorId = DoctorId,
                TierId = TierElite, Status = BookingStatus.Accepted,
                PaymentStatus = PaymentStatus.Paid,
                AmountUsd = 25.00m, PlatformFeeUsd = 1.25m,
                Notes = "Ongoing follow-up. Last session went well, continuing CBT exercises.",
                CreatedAt = now.AddDays(-12), AcceptedAt = now.AddDays(-11)
            },
            // Declined — Standard (shows refunded state and decline reason)
            new DirectBookingRequest
            {
                Id = Guid.NewGuid(), UserId = Guid.NewGuid(),
                UserFullName = "James Harlow", DoctorId = DoctorId,
                TierId = TierStandard, Status = BookingStatus.Declined,
                PaymentStatus = PaymentStatus.Refunded,
                AmountUsd = 10.00m, PlatformFeeUsd = 0.50m,
                Notes = "Looking for support with substance dependency.",
                DeclineReason = DeclineReason.SpecializationMismatch,
                DeclineNote = "This case requires a specialist in addiction therapy. Please seek a provider with that accreditation.",
                CreatedAt = now.AddDays(-3), DeclinedAt = now.AddDays(-3).AddHours(2)
            }
        );
    }

    // ── Session Reports ──────────────────────────────────────────────────────
    //  Two reports for Noah Peterson: session 1 (read-only, > 24h old), session 2 (editable)

    private async Task SeedSessionReportsAsync(CancellationToken ct)
    {
        if (await db.SessionReports.AnyAsync(ct)) return;
        var now = DateTimeOffset.UtcNow;

        db.SessionReports.AddRange(
            new SessionReport
            {
                Id = Guid.NewGuid(),
                BookingId = NoahBookingId, DoctorId = DoctorId, UserId = NoahUserId,
                UserFullName = "Noah Peterson", SessionNumber = 1,
                SessionDate = DateTime.UtcNow.Date.AddDays(-8),
                MoodBefore = 3, MoodAfter = 6,
                GoalsCompleted = 2, GoalsTotal = 3,
                ProgressNotes = "Patient arrived visibly fatigued. We reviewed the previous week's stressors and identified three cognitive distortions linked to his perfectionism at work. He responded well to reframing exercises and expressed willingness to keep a thought diary.",
                TechniquesUsed = new[] { "CBT", "Cognitive reframing", "Thought diary" },
                NextSteps = "Complete the thought diary for at least five work days. Note the trigger, automatic thought, and a balanced alternative each time.",
                CreatedAt = now.AddDays(-8), UpdatedAt = now.AddDays(-8)
            },
            new SessionReport
            {
                Id = Guid.NewGuid(),
                BookingId = NoahBookingId, DoctorId = DoctorId, UserId = NoahUserId,
                UserFullName = "Noah Peterson", SessionNumber = 2,
                SessionDate = DateTime.UtcNow.Date,
                MoodBefore = 5, MoodAfter = 8,
                GoalsCompleted = 3, GoalsTotal = 3,
                ProgressNotes = "Strong progress. Noah completed the thought diary consistently and was able to identify and challenge perfectionist thoughts in real time during a high-pressure project. He reported sleeping better and described feeling more in control.",
                TechniquesUsed = new[] { "CBT", "Breathing techniques", "Journaling", "Behavioural activation" },
                NextSteps = "Continue behavioural activation plan. Add one enjoyable activity per day outside work. Review diary in next session.",
                CreatedAt = now.AddHours(-2), UpdatedAt = now.AddHours(-2)
            }
        );
    }
}
