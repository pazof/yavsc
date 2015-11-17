<%@ Page Title="Devis" Language="C#" Inherits="System.Web.Mvc.ViewPage<Estimate>" MasterPageFile="~/Models/App.master" %>
<%@ Register Assembly="Yavsc.WebControls" TagPrefix="yavsc" Namespace="Yavsc.WebControls" %> 
<asp:Content ContentPlaceHolderID="head" ID="head1" runat="server" >
<script type="text/javascript" src="<%=Url.Content("~/Scripts/stupidtable.js")%>"></script> 
<script>
</script>
</asp:Content>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
 <aside>
    <div id="dfnuser" class="hidden">
   <%= Html.Partial("~/Views/Account/Register.ascx",new RegisterClientModel(),new ViewDataDictionary(ViewData)
        {
            TemplateInfo = new System.Web.Mvc.TemplateInfo
            {
                HtmlFieldPrefix = ViewData.TemplateInfo.HtmlFieldPrefix==""?"ur":ViewData.TemplateInfo.HtmlFieldPrefix+"_ur"
            }
        }) %>
 	</div>
 	 </aside>

    <aside>
   <%= Html.Partial("Writting",new Writting(),new ViewDataDictionary(ViewData)
        {
            TemplateInfo = new System.Web.Mvc.TemplateInfo
            {
                HtmlFieldPrefix = ViewData.TemplateInfo.HtmlFieldPrefix==""?"wr":ViewData.TemplateInfo.HtmlFieldPrefix+"_wr"
            }
        }) %>
       
        <form>
     <div>
        <input type="button" id="btnnew" value="Nouvelle écriture" class="actionlink"/>
        <input type="button" id="btncreate" value="Ecrire" class="actionlink"/>
        <input type="button" id="btnmodify" value="Modifier" class="hidden actionlink"/>
      </div>  </form>
     <tt id="msg" class="hidden message"></tt>

     <style>
     th { cursor:pointer; }
     .row { cursor:pointer; }
     table.tablesorter td:hover { background-color: rgba(0,64,0,0.5); }
     .hidden { display:none; }
     .selected td { background-color: rgba(0,64,0,0.5); }
</style>
     <script>

 jQuery.support.cors = true;  
 function message(msg) { 
  if (msg) { 
  $("#msg").removeClass("hidden");
  $("#msg").text(msg);
  } else { $("#msg").addClass("hidden"); } }

  function GetWritting () {
     	return {
     	Id: Number($("#wr_Id").val()),
     	UnitaryCost: Number($("#wr_UnitaryCost").val()),
     	Count: parseInt($("#wr_Count").val()),
     	ProductReference: $("#wr_ProductReference").val(),
     	Description: $("#wr_Description").val()
     	};
     }

     function wredit(pwrid)
     {
        if (wr_Id.value>0) {
     	$("#wr"+wr_Id.value).removeClass("selected");
     	$("#wr"+wr_Id.value).addClass((wr_Id.value%2==0)?"even":"odd");
     	}
     	$("#wr_Id").val(pwrid);
     	if (pwrid>0) {
    	$("#btncreate").addClass("hidden");
    	$("#btnmodify").removeClass("hidden");
    	$("#btndrop").removeClass("hidden");
    	$("#wr"+wr_Id.value).removeClass((wr_Id.value%2==0)?"even":"odd");
    	$("#wr"+wr_Id.value).addClass("selected");
    	} else {
    	$("#btncreate").removeClass("hidden");
    	$("#btnmodify").addClass("hidden");
    	$("#btndrop").addClass("hidden");
    	}
     } 

     function delRow(e) {
     clearWrittingValidation();
     e.stopPropagation(); // do not edit this row on this click
   // from <row id= ...><tr><td><input type="button" > 
    	var hid=e.delegateTarget.parentNode.parentNode.id;
    	var vwrid = Number(hid.substr(2));
    		
       $.ajax({
            url: "<%=Url.Content("~/api/WorkFlow/DropWritting")%>",
            type: "Get",
            data: { wrid: vwrid }, 
            contentType: 'application/json; charset=utf-8',
            success: function () { 
		      $("#wr"+vwrid).remove(); // removes clicked row
			   wredit(0); // set current form target id to none
				message(false); // hides current message
	//		$("#tbwrts").update();// rebuilds the cache for the tablesorter js
	//		$("#tbwrts").tablesorter( {sortList: [[0,0], [1,0]]} ); // .update();
            },
            error: function (xhr, ajaxOptions, thrownError) {
            	if (xhr.status!=400)
        			message(xhr.status+" : "+xhr.responseText);
			    else message(false);
        		}
        });

      }
	
	function setRow() {
       var wrt = GetWritting();
       clearWrittingValidation();
       $.ajax({
            url: "<%=Url.Content("~/api/WorkFlow/UpdateWritting")%>",
            type: 'POST',
            data: wrt, 
            success: function () { 
		       var cells = document.getElementById("wr"+wrt.Id).getElementsByTagName("TD");
		       cells[0].innerHTML=wrt.Description;
		       cells[1].innerHTML=wrt.ProductReference;
		       cells[2].innerHTML=wrt.Count;
		       cells[3].innerHTML=wrt.UnitaryCost;
			   message(false);
            },
            statusCode: {
            	400: function(data) {
            		$.each(data.responseJSON, function (key, value) {
            		var errspanid = "Err_" + value.key.replace(".","_");
            		var errspan = document.getElementById(errspanid);
            		if (errspan==null)
            			alert('enoent '+errspanid);
            		else 
            			errspan.innerHTML=value.errors.join("<br/>");
                	});
            		}
            	},
             error: function (xhr, ajaxOptions, thrownError) {
            	if (xhr.status!=400)
        			message(xhr.status+" : "+xhr.responseText);
			    else message(false);
        		}
        });
    }
    function addUser()
    {
    	var user={
     	UserName: $("#ur_UserName").val(),
     	Name: $("#ur_Name").val(),
     	Password: $("#ur_Password").val(),
     	Email: $("#ur_Email").val(),
     	Address: $("#ur_Address").val(),
     	CityAndState: $("#ur_CityAndState").val(),
     	ZipCode: $("#ur_ZipCode").val(),
     	Phone: $("#ur_Phone").val(),
     	Mobile: $("#ur_Mobile").val(),
     	IsApprouved: true
     	};
     	clearRegistrationValidation();
        $.ajax({
            url: "<%=Url.Content("~/api/FrontOffice/Register")%>",
            type: "POST",
            data: user,
            success: function (data) { 
            	$("#Client option:last").after($('<option>'+user.UserName+'</option>'));
            	Client.value = user.UserName;
            	onClientChange(Client.value);
            },
            statusCode: {
            	400: function(data) {
            		$.each(data.responseJSON, function (key, value) {
            		var errspanid = "Err_ur_" + value.key.replace("model.","");
            		var errspan = document.getElementById(errspanid);
            		if (errspan==null)
            			alert('enoent '+errspanid);
            		else 
            			errspan.innerHTML=value.errors.join("<br/>");
                	});
            		}
            	},
            error: function (xhr, ajaxOptions, thrownError) {
            	if (xhr.status!=400)
        			message(xhr.status+" : "+xhr.responseText);
			    else message(false);
        		}});
    }
    function clearWrittingValidation() {
    	$("#Err_wr_Description").text("");
            $("#Err_wr_ProductReference").text("");
            $("#Err_wr_UnitaryCost").text("");
            $("#Err_wr_Count").text("");
    }
function clearRegistrationValidation(){
    		$("#Err_ur_Name").text("");
            $("#Err_ur_UserName").text("");
            $("#Err_ur_Mobile").text("");
            $("#Err_ur_Phone").text("");
            $("#Err_ur_Email").text("");
            $("#Err_ur_Address").text("");
            $("#Err_ur_ZipCode").text("");
            $("#Err_ur_CityAndState").text("");
            $("#Err_ur_IsApprouved").text("");
    }

function addRow(){
       var wrt = GetWritting(); // gets a writting object from input controls
		var estid = parseInt($("#Id").val());
        clearWrittingValidation();

        $.ajax({
            url: "<%=Url.Content("~/api/WorkFlow/Write?estid=")%>"+estid,
            type: "POST",
            data: wrt,
            dataType: "json",
            success: function (data) { 
            wrt.Id = Number(data);
             wredit(wrt.Id);
            var wridval = 'wr'+wrt.Id;
            jQuery('<tr/>', { 
            id: wridval,
            "class": 'selected row',
            }).appendTo('#wrts');
            $("<td>"+wrt.Description+"</td>").appendTo("#"+wridval);
            $("<td>"+wrt.ProductReference+"</td>").appendTo("#"+wridval);
            $("<td>"+wrt.Count+"</td>").appendTo("#"+wridval);
            $("<td>"+wrt.UnitaryCost+"</td>").appendTo("#"+wridval);
            var btrm = $("<input type=\"button\" value=\"X\" class=\"actionlink rowbtnrm\"/>");
	        $("<td></td>").append(btrm).appendTo("#"+wridval); 
            btrm.click(function (e) {delRow(e);});
            $("#"+wridval).click(function(ev){onEditRow(ev);});
            message(false);
            },
            statusCode: {
            	400: Yavsc.onAjaxBadInput
            	},
            	error: function (xhr, ajaxOptions, thrownError) {
            	if (xhr.status!=400)
        			message(xhr.status+" : "+xhr.responseText);
			    else message(false);
        		}
            	});
    }

     function onEditRow(e) {
    	var cells = e.delegateTarget.getElementsByTagName("TD");
    	var hid=e.delegateTarget.id;
    	var vwrid = Number(hid.substr(2));

    	wredit(vwrid);
    	$("#wr_Description").val(cells[0].innerHTML);
    	$("#wr_ProductReference").val(cells[1].innerHTML);
    	$("#wr_Count").val(cells[2].innerHTML);
    	$("#wr_UnitaryCost").val(Number(cells[3].innerHTML.replace(",",".")));
    
    }
  
 $(document).ready(function () {
    $("#btnnewuser").click(addUser);
    $("#btncreate").click(addRow);
    $("#btnmodify").click(setRow);
    $(".row").click(function (e) {onEditRow(e);});
    $(".rowbtnrm").click(function (e) {delRow(e);});

	$("#btnnew").click(function () {
		wredit(0);
		$("#wr_Description").val("");
    	$("#wr_ProductReference").val("");
    	$("#wr_Count").val(1);
    	$("#wr_UnitaryCost").val(0);
	});

	$("#tbwrts").stupidtable();

});
    </script>

    <a class="actionlink" href="<%=Url.Content(Yavsc.WebApiConfig.UrlPrefixRelative)%>/FrontOffice/EstimateToTex?id=<%=Model.Id%>"><%= LocalizedText.Tex_version %></a>
    <a class="actionlink" href="<%=Url.Content(Yavsc.WebApiConfig.UrlPrefixRelative)%>/FrontOffice/EstimateToPdf?id=<%=Model.Id%>"><%= LocalizedText.Pdf_version %></a>
    </aside>

<%= Html.ValidationSummary("Devis") %>

<% using  (Html.BeginForm("Estimate","FrontOffice")) { %>
<%= Html.LabelFor(model => model.Title) %>:<%= Html.TextBox( "Title" ) %>
<%= Html.ValidationMessage("Title", "*") %>
<br/>
<%= Html.Hidden ("Responsible") %>

<%= Html.LabelFor(model => model.Client) %>:
   
   <% Client.Value = Model.Client ; %>
	 <yavsc:InputUserName
	  id="Client"
	  name="Client"
	   emptyvalue="*nouvel utilisateur*"
	 onchange="onClientChange(this.value);"
	   runat="server"  >
	 </yavsc:InputUserName> 
 	<script>
 	function onClientChange(newval)
 	{
 		if (newval=='')
 			$("#dfnuser").removeClass("hidden");
 		else
 			$("#dfnuser").addClass("hidden");
 	}
 	</script>
 	<%= Html.ValidationMessage("Client", "*") %>
<br/>
<%= Html.LabelFor(model => model.Description) %>:<%=Html.TextArea( "Description") %>
<%= Html.ValidationMessage("Description", "*") %>
<br/>
<%= Html.Hidden( "Id" ) %>
<br/>
<% if (Model.Id==0) { %>
   <input type="submit" name="submit" value="Create"/>
<% } else { %>
   <input type="submit" name="submit" value="Update"/>
<% } %>

   <% if (Model.Id>0) { %>
<table  id="tbwrts">
<thead>
<tr>
<th data-sort="string"><%=Yavsc.Model.LocalizedText.Description%></th>
<th data-sort="string"><%=Yavsc.Model.LocalizedText.Product_reference%></th>
<th data-sort="int"><%=Yavsc.Model.LocalizedText.Count%></th>
<th data-sort="float"><%=Yavsc.Model.LocalizedText.Unitary_cost%></th>
  
</tr>
</thead>
<tbody id="wrts">
<% int lc=0;
   if (Model.Lines!=null)
   foreach (Writting wr in Model.Lines) { lc++; %>
<tr class="<%= (wr.Id%2==0)?"even ":"odd " %>row" id="wr<%=wr.Id%>">
<td><%=wr.Description%></td>
<td><%=wr.ProductReference%></td>
<td><%=wr.Count%></td>
<td><%=wr.UnitaryCost%></td>
   <td>
        <input type="button" value="X" class="actionlink rowbtnrm"/>
    </td>
</tr>
<%    } %>
</tbody>
</table>
<%  } %>
<% } %>
   

  
   </asp:Content>