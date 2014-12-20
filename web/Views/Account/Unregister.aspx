
<%@ Page Title="Unregister" Language="C#" Inherits="System.Web.Mvc.ViewPage" MasterPageFile="~/Models/App.master" %>

<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
Warning: This will delete all of your data here, your profile, your posts and other data.
<% using(Html.BeginForm("Unregister", "Account")) { %>
   <label for="confirmed">Unregister</label> 
   <%=Html.CheckBox("confirmed")%>
   <input type="submit"/>
   <% } %>
</asp:Content>
