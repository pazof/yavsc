<%@ Page Title="User List" Language="C#" Inherits="System.Web.Mvc.ViewPage<System.Web.Security.MembershipUserCollection>" MasterPageFile="~/Models/App.master" %>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">

 <ul>
       <%foreach (MembershipUser user in Model){ %>

       <li><%=user.UserName%> <%=user.Email%> <%=(!user.IsApproved)?"(Not Approuved)":""%>  <%=user.IsOnline?"Online":"Offline"%>
<% if (Roles.IsUserInRole("Admin")) { %>
	 <%= Html.ActionLink("Supprimer","RemoveUserQuery", new { username = user.UserName }, new { @class="actionlink" } ) %>	
<% } %>
         </li>

       <% }%>
</ul>

</asp:Content>

