*MSPK V07.00 Silver Monkey
*MonkeySpeak Script File
*Created by Wiren
 
**SECTION**  Anti-AFK
(0:0) When the bot starts,
    (5:300) create timer 1 to go off every 150 second(s).
 
(0:300) When timer 1 goes off,
    (5:614) turn the bot clock-wise one space.
**SECTION**  Arrive Whisper To Wiren
(0:0) When the bot starts,
    (5:100) set variable %arrivalwhisper to {OFF}.
 
(0:20) When someone whispers {aw on},
  (1:10) and the triggering furre's name is {Wiren},
  (1:104) and variable %arrivalwhisper equals {OFF},
    (5:100) set variable %arrivalwhisper to {ON}.
    (5:16) whisper {Arrival Whispers <B>%arrivalwhisper</B>.} to furre named {Wiren}.
 
(0:20) When someone whispers {aw off},
  (1:10) and the triggering furre's name is {Wiren},
  (1:104) and variable %arrivalwhisper equals {ON},
    (5:100) set variable %arrivalwhisper to {OFF}.
    (5:16) whisper {Arrival Whispers <b>%arrivalwhisper</b>.} to furre named {Wiren}.
 
(0:27) When someone emits something with {W0413} in it,
  (1:104) and variable %arrivalwhisper equals {ON},
    (5:18) set variable %aw to the last message seen.
    (5:122) chop off the beginning of variable %aw, removing the first 40 characters of it.
    (5:123) chop off the end of variable %aw, removing the last 7 characters of it.
    (5:16) whisper {%aw} to furre named {Wiren}.
**SECTION**  Auto Bot Seat
(0:27) When someone emits something with {830036500} in it,
    (5:10) say {!bot}.
 
 
**SECTION**  Auto Join
(0:40) When someone requests to summon the bot,
  (1:10) and the triggering furre's name is {Wiren},
    (5:10) say {`join}.
 
**SECTION**  Auto Summon
(0:42) When someone requests to join the bot,
  (1:51) and the triggering furre is not on the banish list,
    (5:10) say {`summon}.
 
 
**SECTION**  Auto Whisper
(0:19) When someone whispers something,
  (1:11) and the triggering furre's name is not {Wiren},
    (5:15) whisper {Bot Programmed by Wiren. To Join Type <b>`join Dream Weaving Tutorial</b>. <i>Automated Response</i>} to the triggering furre.
 
**SECTION**  Ban
(0:17) When someone whispers something with {gban} in it,
  (1:10) and the triggering furre's name is {Wiren},
    (5:18) set variable %ban to the last message seen.
    (5:122) chop off the beginning of variable %ban, removing the first 5 characters of it.
  (1:52) and the furre named {%ban} is not on the banish list,
  (1:10) and the triggering furre's name is {Wiren},
    (5:54) banish the furre named {%ban}.
    (5:16) whisper {%ban has been added to the <b>BANNED LIST</b>} to furre named {Wiren}.
  (1:53) and the furre named {%ban} is on the banish list,
  (1:10) and the triggering furre's name is {Wiren},
    (5:16) whisper {%ban is already on the <b>BANNED LIST</b>} to furre named {Wiren}.
 
(0:17) When someone whispers something with {unban} in it,
  (1:10) and the triggering furre's name is {Wiren},
    (5:18) set variable %unban to the last message seen.
    (5:122) chop off the beginning of variable %unban, removing the first 6 characters of it.
  (1:53) and the furre named {%ban} is on the banish list,
  (1:10) and the triggering furre's name is {Wiren},
    (5:58) unbanish the furre named {%ban}.
    (5:16) whisper {%unban has been removed to the <b>BANNED LIST</b>} to furre named {Wiren}.
  (1:52) and the furre named {%unban} is not on the banish list,
  (1:10) and the triggering furre's name is {Wiren},
    (5:16) whisper {%unban is not on the <b>BANNED LIST</b>} to furre named {Wiren}.
 
**SECTION**  Disconnect
(0:20) When someone whispers {Disconnect},
  (1:10) and the triggering furre's name is {Wiren},
    (5:41) Disconnect the bot from the Furcadia game server.
 
**SECTION**  Eject
(0:17) When someone whispers something with {eject} in it,
  (1:10) and the triggering furre's name is {Wiren},
    (5:18) set variable %eject to the last message seen.
    (5:122) chop off the beginning of variable %eject, removing the first 6 characters of it.
    (5:10) say {eject %eject}.
    (5:16) whisper {%eject has been <b>EJECTED</b>} to furre named {Wiren}.
**SECTION**  Emit
(0:17) When someone whispers something with {emitloud } in it,
  (1:10) and the triggering furre's name is {Wiren},
    (5:18) set variable %emitloud to the last message seen.
    (5:122) chop off the beginning of variable %emitloud, removing the first 8 characters of it.
    (5:14) Emitloud {Wiren: %emitloud}.
**SECTION**  Reupload
(0:20) When someone whispers {reupload},
  (1:10) and the triggering furre's name is {Wiren},
    (5:10) say {`fdl furc://allegriaisland}.
    (5:300) create timer 3 to go off every 2 second(s).
 
(0:300) When timer 3 goes off,
    (5:10) say {`use}.
 
(0:27) When someone emits something with {Vinca Crossroads} in it,
    (5:301) stop timer 3.
    (5:16) whisper {Bot Arrived At "Vinca Crossroads"} to furre named {Wiren}.
 
 
(0:300) When timer 4 goes off,
    (5:625) Move the bot  in this sequence {3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3} (one, sw, three, se, seven, nw, nine, or ne)
    (5:301) stop timer 4.
 
 
**SECTION**  Share List
(0:20) When someone whispers {list},
  (1:10) and the triggering furre's name is {Wiren},
    (5:10) say {ps get dream}.
    (5:300) create timer 2 to go off every 1 second(s).
 
 
(0:300) When timer 2 goes off,
    (5:18) set variable %list to the last message seen.
    (5:122) chop off the beginning of variable %list, removing the first 20 characters of it.
    (5:16) whisper {Shared List: %list} to furre named {Wiren}.
    (5:301) stop timer 2.
**SECTION**  Share/Unshare
(0:17) When someone whispers something with {gshare} in it,
  (1:10) and the triggering furre's name is {Wiren},
    (5:18) set variable %share to the last message seen.
    (5:122) chop off the beginning of variable %share, removing the first 7 characters of it.
    (5:10) say {!share %share}.
 
(0:27) When someone emits something with {S1109} in it,
    (5:18) set variable %sharemessage to the last message seen.
    (5:122) chop off the beginning of variable %sharemessage, removing the first 33 characters of it.
    (5:123) chop off the end of variable %sharemessage, removing the last 7 characters of it.
    (5:16) whisper {%share added to the <b>SHARE LIST</b>} to furre named {Wiren}.
    (5:100) set variable %share to {Blankplaceholder}.
 
(0:27) When someone emits something with {%share} in it,
    (5:18) set variable %notpresent to the last message seen.
    (5:122) chop off the beginning of variable %notpresent, removing the first 33 characters of it.
    (5:123) chop off the end of variable %notpresent, removing the last 7 characters of it.
    (5:16) whisper {%notpresent} to furre named {Wiren}.
    (5:100) set variable %share to {Blankplaceholder}.
 
(0:17) When someone whispers something with {unshare} in it,
  (1:10) and the triggering furre's name is {Wiren},
    (5:18) set variable %unshare to the last message seen.
    (5:122) chop off the beginning of variable %unshare, removing the first 8 characters of it.
    (5:10) say {!unshare %unshare}.
    (5:16) whisper {%unshare removed from <b>SHARE LIST</b>} to furre named {Wiren}.
    (5:10) say {unshare %unshare}.
    (5:100) set variable %unshare to {Blankplaceholder}.
 
(0:17) When someone whispers something with {!remove} in it,
  (1:10) and the triggering furre's name is {Wiren},
    (5:18) set variable %remove to the last message seen.
    (5:122) chop off the beginning of variable %remove, removing the first 8 characters of it.
    (5:10) say {!remove %remove}.
**SECTION**  Speak
(0:17) When someone whispers something with {say} in it,
  (1:10) and the triggering furre's name is {Wiren},
    (5:18) set variable %speak to the last message seen.
    (5:122) chop off the beginning of variable %speak, removing the first 4 characters of it.
    (5:10) say {%speak}.
 
**SECTION**  Wiren Command List
(0:20) When someone whispers {commands},
  (1:10) and the triggering furre's name is {Wiren},
    (5:15) whisper {aw on, aw off, gban <user>, unban <user>, disconnect, eject <user>, list, gshare <user>, unshare <user>, say <message>.} to the triggering furre.
 
*Endtriggers* 8888 *Endtriggers*