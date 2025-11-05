using FoundryLocal.Core.ViewModels;

namespace FoundryLocal.Core;

public static class SampleData
{
    public static StudentMessageViewModel[] GetSampleStudentProfiles() => [
        new StudentMessageViewModel
        {
            StudentName = "Sarah Johnson",
            StudentId = "SJ2024001",
            ReceivedDate = DateTime.Now.AddHours(-2),
            Subject = "Financial Aid Eligibility Question",
            MessageText = "Hi! I'm Sarah, a pre-med student with a 3.8 GPA. I'm a U.S. citizen with SSN 123-45-6789 and I graduated from high school. I'm wondering if I qualify for federal financial aid? I have good grades but I'm worried about the requirements.",
            IsUrgent = false
        },
        new StudentMessageViewModel
        {
            StudentName = "Mike Rodriguez",
            StudentId = "MR2024002",
            ReceivedDate = DateTime.Now.AddHours(-5),
            Subject = "Previous Loan Issues - Aid Eligibility",
            MessageText = "Hello, I'm Mike Rodriguez. I'm an engineering student but I have some issues with my previous federal loans. My GPA is around 2.1. I'm a permanent resident with SSN 234-56-7890 and I have my GED. Can I still get financial aid?",
            IsUrgent = true
        },
        new StudentMessageViewModel
        {
            StudentName = "Ashley Chen",
            StudentId = "AC2024003",
            ReceivedDate = DateTime.Now.AddHours(-1),
            Subject = "Low Grades Impact on Aid",
            MessageText = "Hi there! I'm Ashley, studying business. My grades haven't been great lately - my GPA is 1.2 and I have some courses with really low grades. I'm a U.S. citizen and high school graduate. How does this affect my financial aid eligibility?",
            IsUrgent = false
        },
        new StudentMessageViewModel
        {
            StudentName = "David Kim",
            StudentId = "DK2024004",
            ReceivedDate = DateTime.Now.AddHours(-8),
            Subject = "International Student Aid Question",
            MessageText = "Hello, my name is David Kim. I'm an international student from South Korea studying computer science. My GPA is 3.5 and I completed high school. I don't have an SSN yet. What financial aid options are available for someone in my situation?",
            IsUrgent = false
        },
        new StudentMessageViewModel
        {
            StudentName = "Maria Gonzalez",
            StudentId = "MG2024005",
            ReceivedDate = DateTime.Now.AddMinutes(-30),
            Subject = "URGENT: Aid Deadline Approaching",
            MessageText = "Hi, this is Maria Gonzalez. I'm a U.S. citizen with SSN 456-78-9012, high school graduate, GPA 3.2. I need to know about financial aid ASAP as deadlines are approaching. I have no previous loan issues. Can you help me understand what I qualify for?",
            IsUrgent = true
        },
        new StudentMessageViewModel
        {
            StudentName = "James Thompson",
            StudentId = "JT2024006",
            ReceivedDate = DateTime.Now.AddDays(-1),
            Subject = "GED and Financial Aid Eligibility",
            MessageText = "Hello, I'm James Thompson. I got my GED instead of graduating traditionally. I'm a U.S. citizen with SSN 567-89-0123. My current GPA in college is 2.8. I want to know if having a GED affects my federal financial aid eligibility.",
            IsUrgent = false
        }
    ];
}
