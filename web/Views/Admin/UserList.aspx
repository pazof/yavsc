<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<System.Web.Security.MembershipUserCollection>" MasterPageFile="~/Models/AppAdmin.master" %>
<asp:Content ContentPlaceHolderID="init" ID="init1" runat="server">
	<% Page.Title = LocalizedText.User_List; %>
</asp:Content>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
 <ul>
       <%foreach (MembershipUser user in Model){ %>
       <li><%=user.UserName%> (created <%=user.CreationDate.ToString("D")%>) <%=user.Email%> <%=(user.IsApproved)?"":"("+LocalizedText.Not_Approuved+")"%>  <%=user.IsOnline?LocalizedText.Online:LocalizedText.Offline%>
<% if (Roles.IsUserInRole("Admin")) { %>
	 <%= Html.ActionLink(LocalizedText.Remove,"RemoveUser", new { username = user.UserName }, new { @class="actionlink" } ) %>	
<% } %>
       </li><% }%>
</ul>

</asp:Content>

