-- select * from "AspNetRoles";
-- select * from "AspNetUserRoles";
select "AspNetUsers"."UserName", "AspNetRoles"."Name" from "AspNetUsers"
inner join "AspNetUserRoles" on "AspNetUserRoles"."UserId" = "AspNetUsers"."Id" 
inner join "AspNetRoles" on "AspNetRoles"."Id" = "AspNetUserRoles"."RoleId"
;
