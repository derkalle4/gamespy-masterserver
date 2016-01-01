Module constants
    'Generic
    Public Const PRODUCT_NAME As String = "GameMaster Master Server"
    Public Const CFG_DIR As String = "/cfg"                     'Dir for Config files
    Public Const CFG_FILE As String = "/core.xml"               'Main Config File

    'Listrequest
    Public Const PARAM_UNKNOWN_REPLACEMENT As String = "?"      'Replacement for "NULL"-params
    Public Const PARAM_MAX_LOCALIP_SEEK As Int32 = 20           'Max. localips
    Public Const PARAM_NO_PLAYERNAME As String = "[null]"       'Replacement for "NULL"-names

    'TCP-Protocol
    Public Const TCP_CLIENT_TIMEOUT As Int32 = 10000            'Timeout for TCP-clients
    Public Const TCP_CLIENT_PSH_MAXCOUNT As Int32 = 10          'Max tcp-push p. client
    Public Const TCP_CLIENT_PSH_SLEEP As Int32 = 10             'Min. sleeptime btwn. PSH

    'Master-Protocol
    Public Const GS_MASTER_CMD_AVAILIABLE As Byte = &H9         'Check if services are up
    Public Const GS_MASTER_CMD_QUERY As Byte = &H0

    Public Const GS_MASTER_CMD_REGISTER As Byte = &H3           'Register Server
    Public Const GS_MASTER_CMD_HEARTBEAT As Byte = &H8          'Keepalive Rq.

    Public Const GS_MASTER_CMD_MESSAGE As Byte = &H6            'Message-Fwd.
    Public Const GS_MASTER_CMD_MESSAGE_ACK As Byte = &H5        'Message received

    Public Const GS_MASTER_CMD_HANDSHAKE As Byte = &H2          'Firewall check
    Public Const GS_MASTER_CMD_HANDSHAKE_ACK As Byte = &H5      'Handshake response

    Public Const GS_MASTER_CMD_CHALLENGE As Byte = &H1          'Request token
    Public Const GS_MASTER_CMD_CHALLENGE_RES As Byte = &H1      'Got Challenge
    Public Const GS_MASTER_CMD_CHALLENGE_ACK As Byte = &HA      'Challenge was ok

    Public Const GS_HANDSHAKE_STRING As String = "Have a beer?" 'Payload for Handshake

    'Serverbrowsing-Protocol
    Public Const GS_MS_CLIENT_CMD_LIST_REQ As Byte = &H0        'Std. list request /crypt init
    Public Const GS_MS_CLIENT_CMD_SERVERINFO As Byte = &H1      'Detail-info for server
    Public Const GS_MS_CLIENT_CMD_MESSAGE As Byte = &H2         'Forward message
    Public Const GS_MS_CLIENT_CMD_KEEPALIVE As Byte = &H3       'Keep connection open
    Public Const GS_MS_CLIENT_CMD_MAPLOOP As Byte = &H4
    Public Const GS_MS_CLIENT_CMD_PLAYERSEARCH As Byte = &H5

    Public Const GS_MS_SERVER_CMD_PUSHKEYS As Byte = &H1
    Public Const GS_MS_SERVER_CMD_PUSHSERVER As Byte = &H2      'Push Server info
    Public Const GS_MS_SERVER_CMD_KEEPALIVE As Byte = &H3       'Keepalive reply
    Public Const GS_MS_SERVER_CMD_DROPSERVER As Byte = &H4      'Remove Server
    Public Const GS_MS_SERVER_CMD_MAPLOOP As Byte = &H5
    Public Const GS_MS_SERVER_CMD_PLAYERSEARCH As Byte = &H6

    Public Const GS_CRYPT_CHALLENGELEN As Byte = 8

    Public Const GS_SERVER_NAT_ID As Byte = 126                 'Server behind restrictive NAT
    Public Const GS_SERVER_RAW_ID As Byte = 21                  'Server directly to WAN
    Public Const GS_SERVER_DIRECT_ID As Byte = 85               'Server behind open NAT

    Public Const GS_FLAG_UNSOLICITED_UDP As Byte = 1 << 0           'n. UDP-Query
    Public Const GS_FLAG_PRIVATE_IP As Byte = 1 << 1                'NAT
    Public Const GS_FLAG_CONNECT_NEGOTIATE_FLAG As Byte = 1 << 2    'restrictive NAT
    Public Const GS_FLAG_ICMP_IP As Byte = 1 << 3                   'allow ICMP-Ping
    Public Const GS_FLAG_NONSTANDARD_PORT As Byte = 1 << 4          'send custom Port
    Public Const GS_FLAG_NONSTANDARD_PRIVATE_PORT As Byte = 1 << 5  'send custom lPort
    Public Const GS_FLAG_HAS_KEYS As Byte = 1 << 6                  '
    Public Const GS_FLAG_HAS_FULL_RULES As Byte = 1 << 7            'contains full set of vars

    Public Const GS_FLAG_SEND_FIELDS_FOR_ALL As Byte = 1 << 0
    Public Const GS_FLAG_NO_SERVER_LIST As Byte = 1 << 1
    Public Const GS_FLAG_PUSH_UPDATES As Byte = 1 << 2
    Public Const GS_FLAG_SEND_GROUPS As Byte = 1 << 5
    Public Const GS_FLAG_NO_LIST_CACHE As Byte = 1 << 6
    Public Const GS_FLAG_LIMIT_RESULT_COUNT As Byte = 1 << 7

    Public GS_SERVICE_MASTER_PREFIX As Byte() = {&HFE, &HFD}    'Prefix for Masterserver
    Public GS_SERVICE_NATNEG_PREFIX As Byte() = {&HFD, &HFC}    'Prefix for Natnegserver

    Public GS_NATNEG_HEADER As Byte() = {&H1E, &H66, &H6A, &HB2} 'header for Natneg-Packets

    'MySQL
    Public Const MYSQL_GAMESERVER_TABLE_NAME As String = "gameserver"   'Table to store the servers in
    Public Const MYSQL_SERVERKEY_TABLE_NAME As String = "serverkeys"    'Table containing the keys
    Public Const MYSQL_PLAYERROW_DELAY As Int32 = 1                     'Max. update rate /per s.

    'Multiserver-Protocol
    Public Const P2P_CMD_PING As Byte = &H0                     'Echo request
    Public Const P2P_CMD_PONG As Byte = &H1                     'Echo reply

    Public Const P2P_CMD_SENDMESSAGE As Byte = &H10             'Forward UDP packet

    'Debugging
    Public Const DEBUGMODE_ENABLE As Boolean = False            'Enables verbose logging w/o config
End Module