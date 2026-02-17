# HSMS
Housing Society Management System

*** Features ***
1. Login/signup with email/phone/password and role-based access (resident vs admin). 3 types of admin... President, Director and Treasurer. 

2. Home and Admin dashboard: notices, upcoming events, and unpaid fees.

3. Member directory: List/search users with profiles. Only Admin can add/edit/delete member. Each member can view read-only details of other member.

4. Member detail: Admin can add/edit/delete member. Member can edit self detail. Also, it is possible that some of the home/flats are given on rent. Those flats can be marked as on rent and and these flats will be displayed in different color in Member Directory. For Rented flats details of both owners and rentee will be stored.

5. Notices: Admin can post, wherease residents can view view to notifications. Only admin can add/edit/delete notices.

6. Events calendar. Admin can add/edit/delete events.

7. Payment: Details of Payment made or received. Admin and member can enter detail of Payment made by them (e.g. Payment towards annual maintenance). Status of new payment detail entered by member will be by default "Pending For Approval" and only admin can approve/reject it. Payment type can be Cash/Cheque/Online.

8. Accounting - Maintain Income and Expense. Admin can enter different Income and Expenses. Member can view these details but cannot edit it.

*** Tech Stack ***
Backend: C# .NET 8
Database: PostgreSQL (Supabase)
Frontend: Flutter