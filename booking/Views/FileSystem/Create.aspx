<%@ Page Title="File posting" Language="C#" MasterPageFile="~/Models/App.master" Inherits="System.Web.Mvc.ViewPage<FileUpload>" %>

<asp:Content ID="MainContentContent" ContentPlaceHolderID="MainContent" runat="server">
<% using(Html.BeginForm("Create", "FileSystem", FormMethod.Post, new { enctype = "multipart/form-data" })) { %>
    <div>

        <input type="file" ID="AFile" multiple="multiple" runat="server"/>
        <%= Html.ValidationMessage("AFile") %>

                             <br />
      
        <input type="submit" name="Envoyer" />

        <br />
        <asp:Label ID="Label1" 
                   runat="server"/>
       
        </div>
  <% } %>
</asp:Content>
