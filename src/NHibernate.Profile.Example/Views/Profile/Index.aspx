<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage" MasterPageFile="~/Views/Shared/Site.Master" %>

<asp:Content ID="profileHead" ContentPlaceHolderID="head" runat="server">
    <title>User Profile</title>
</asp:Content>
<asp:Content ID="profileContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>
        User Profile</h2>
    <%Html.BeginForm(); %>
    <div>
        <fieldset>
            <legend>Parameters</legend>
            <p>
                <label for="TimeZone">
                    Time Zone:</label>
                <%= Html.DropDownList("TimeZone") %>
            </p>
            <p>
                <input type="submit" value="Save" />
            </p>
        </fieldset>
    </div>
    <%Html.EndForm(); %>
</asp:Content>