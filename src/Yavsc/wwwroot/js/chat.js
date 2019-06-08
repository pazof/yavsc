window.ChatHubHandler = (function ($) {

  /*
  # chat control ids
  ## inputs

   * msg_<rname> : input providing a room message
   * pv_<uname>  : input providing a private message
   * command : input providing server command
   * roomname : input (hidden or not) providing a room name

  ## persistent containers

   * chatview : the global container
   * rooms : container from room displays
   * pvs : container from pv displays
   * server : container for server messages

  ## temporary containers

  * room_<rname> : room message list
  * pv_<uname> : private discussion display

  ## css classes

  * form-control
  * btn btn-default

  */
  $.fn.filterByData = function (prop, val) {
    return this.filter(function () { return $(this).data(prop) == val; });
  };
  var ChatView = function ($view, full) {

    if (!full) throw new Error('not implemented');

    // the channel list
    var chans = [];
    // private chat list
    var userlist = [];

    var frontChanId;

    var ulist = $('<ul></ul>').addClass('userlist');
    var notifications = $('<ul></ul>').addClass('notifs');

    ulist.appendTo($view);
    notifications.appendTo($view);


    var onUserDisconnected = function (uname) {
      $('#u' + uname).remove();
    };

    var onUserConnected = function (username) {
      addChatUser(username);
    };

    var chat = $.connection.chatHub;
    // Create a function that the hub can call back to display messages.
    chat.client.addMessage = function (name, room, message) {
      // Add the message to the page.
      var $userTag = $('<a>' + htmlEncode(name) + '</a>').click(function() {
        buildPv(name);
      });
      var $li = $('<li class="discussion"></li>');
      $userTag.appendTo($li);
      $li.append(' ' + htmlEncode(message));
      $li.appendTo($('#r' + room));
    };

    chat.client.addPV = function (name, message) {
      if (!$('#mute').prop('checked')) {
        audio.play();
      }
      buildPv(name);
      // Add the pv to the page.
      $('#u' + name).append('<li class="pv"><strong>' + htmlEncode(name) + '</strong>: ' + htmlEncode(message) + '</li>');
    };

    chat.client.notifyRoom = function (tag, targetid, message) {
      // Add the notification to the page.
      if (tag === 'connected' || tag === 'reconnected') {
        onUserConnected(targetid, message);
        return;
      } else if (tag === 'disconnected') {
        onUserDisconnected(targetid, message);
        return;
      }
      // eslint-disable-next-line no-warning-comments
      // TODO reconnected userpart userjoin deniedpv
      $('<li></li>').addClass(tag).append(tag + ': ' + targetid + ' ').append(message).addClass(tag).appendTo($('#room_' + targetid));
    };

    chat.client.notifyUser = function (tag, targetid, message) {
      // Add the notification to the page.
      if (tag === 'connected' || tag === 'reconnected') {
        onUserConnected(targetid, message);
        return;
      } else if (tag === 'disconnected') {
        onUserDisconnected(targetid, message);
        return;
      }

      // eslint-disable-next-line no-warning-comments
      // TODO userpart userjoin deniedpv
      $('<li></li>').append(tag + ': ' + targetid + ': ').append(message).addClass(tag).appendTo(notifications);
    };

    var setActiveChan = function (chanId) {
      if (frontChanId != chanId) {
        $('#sel_' + frontChanId).addClass('btn-primary');
        $('#v' + frontChanId).addClass('hidden');
      }
      frontChanId = chanId;
      $('#sel_' + chanId).removeClass('btn-primary');
      $('#v' + chanId).removeClass('hidden');
      $('#inp_' + chanId).focus();
    };

    var chatbar = $('<div class="chatbar"></div>');

    var roomjoin = $('<div class="chatctl" class="form-control"></div>');

    var roomlist = $('<div class="roomlist"></div>');
    roomlist.appendTo(chatbar);
    $('<label for="channame">Join&nbsp;:</label>')
      .appendTo(roomjoin);
    var chanName = $('<input id="channame" title="channel name" hint="yavsc"  >');
    chanName.appendTo(roomjoin);
    roomjoin.appendTo(chatbar);

    chatbar.appendTo($view);
    var chatlist = $('<div class="chatlist" ></div>');
    chatlist.appendTo($view);

    var buildChan = function (chdp, chanType, chanName, sendCmd) {
      var chanId = chanType + chanName;
      var roomTag = $('<a>' + chdp + chanName + '</a>').addClass('btn');
      roomTag.prop('id', 'sel_' + chanId).click(function () {
        setActiveChan(chanId);
        $(this).removeClass('btn-primary');
      });

      roomTag.appendTo(roomlist);
      var roomview = $('<div></div>').addClass('container');
      roomview.appendTo(chatlist);
      roomview.prop('id', 'v' + chanId);
      var msglist = $('<ul></ul>').addClass('mesglist');
      msglist.prop('id', chanId);
      msglist.appendTo(roomview);
      $('<input type="text">')
        .prop('id', 'inp_' + chanId)
        .prop('enable', false)
        .prop('hint', 'hello')
        .prop('title', 'send to ' + chanName)
        .addClass('form-control')
        .keydown(function (ev) {
          if (ev.which == 13) {
            if (this.value.length == 0) return;
              sendCmd(chanName, this.value);
            // chat.server.send(chanName, this.value);
            this.value = '';
          }
        }).appendTo(roomview);
        if (chanType == 'r') chans.push(chanName);
        else if (chanType == 'u' || chanType == 'a') userlist.push(chanName);
      setActiveChan(chanId);
    };
    var buildRoom = function (roomName) {
      if (chans.some(function(cname) { return cname == roomName; })) setActiveChan('r' + roomName);
      else buildChan('#', 'r', roomName, chat.server.send);
    };
    var buildPv = function (userName) {
      if (userlist.some(function(uname) { return uname == userName; })) setActiveChan('u' + userName);
      else
        if (userName[0] == '?') buildChan('@?', 'a', userName.slice(1), chat.server.sendPV);
        else buildChan('@', 'u', userName, chat.server.sendPV);
    };

    $view.data('chans').split(',').forEach(function (chan) {
      buildRoom(chan);
    });

    /*var getUsers = function () {
      $.get('/api/chat/users').done(function (users) {
        $.each(users, function () {
          var user = this;
          addChatUser(user.UserName);
        });
      });
    };*/
    // eslint-disable-next-line no-warning-comments
    // TODO only query user list for a given channel, and only for a participant
    function onCx() {
      setTimeout(function () {
        chans.forEach(function (room) {
          chat.server.join(room).done(function (chatInfo) {
            setActiveChan('r' + chatInfo.Name);
          });
        });
      }, 120);

      $view.removeClass('disabled');
    }

    function onDisCx() {
      $view.addClass('disabled');
    }

    // Start the connection.
    $.connection.hub.start().done(function () {
      onCx();
    });

    $.connection.hub.disconnected(function () {
      onDisCx();
      setTimeout(function () {
          $.connection.hub.start().done(function () {
            onCx();
          });
        }, 30000); // Re-start connection after 30 seconds
    });

    chanName.keydown(function (event) {
      if (event.which == 13) {
        if (this.value.length == 0) return;
        buildRoom(this.value);
        chat.server.join(this.value).done(function (chatInfo) {
          setActiveChan('r' + chatInfo.Name);
        });
        this.value = '';
      }
      // else TODO showRoomInfo(this.value);
    });
    // eslint-disable-next-line no-warning-comments
    // TODO get this data from the chatview element
    var audio = new Audio('/sounds/bell.mp3');
    $('#command').keydown(function (event) {
      if (event.which == 13) {
        // eslint-disable-next-line no-warning-comments
        // TODO server command: sendCommand()
      }
    });

    var addChatUser = function (uname) {

      $('#u' + uname).remove();
      // ulist.remove("li.user[data='"+uname+"']");

      $('<li class="user"><img src="/Avatars/' + uname + '.xs.png"> ' + uname + '</li>')
        .prop('id', 'u_' + uname)
        .css('cursor', 'pointer')
        .click(function () {
           buildPv(uname);
          })
        .appendTo(ulist);
    };

    // This optional function html-encodes messages for display in the page.
    function htmlEncode(value) {
      var encodedValue = $('<div />').text(value).html();
      return encodedValue;
    }

    // FIXME cx clean shutdown
    // $(window).unload(function () {  });

  };

  $(document).ready(function ($) {
    ChatView($('#chatview'), true);
  });

})(window.jQuery);
