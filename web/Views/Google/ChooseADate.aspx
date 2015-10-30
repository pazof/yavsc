<%@ Page Title="Google calendar usage" Language="C#" Inherits="System.Web.Mvc.ViewPage<IFreeDateSet>" MasterPageFile="~/Models/App.master" %>

<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">

<% using ( Html.BeginForm("ChooseAStartingDate","Google") ) { %>
<% foreach (Period e in Model.Values) { %>
<input type="radio" name="datechoice" value="du <%=e.Start%> au <%=e.End%>" />
<% } %>
<input type="hidden" name="returnUrl" id="returnUrl" value="<%=Html.Encode(ViewData["returnUrl"])%>">
<input type="submit">
<% } %>

</asp:Content>
