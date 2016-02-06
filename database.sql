-- phpMyAdmin SQL Dump
-- version 4.2.11
-- http://www.phpmyadmin.net
--
-- Host: localhost
-- Erstellungszeit: 06. Feb 2016 um 21:28
-- Server Version: 5.6.21
-- PHP-Version: 5.6.3

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;

--
-- Datenbank: `master`
--

-- --------------------------------------------------------

--
-- Tabellenstruktur für Tabelle `gameserver`
--

CREATE TABLE IF NOT EXISTS `gameserver` (
`id` int(11) NOT NULL,
  `masterserver` int(11) NOT NULL,
  `dynamic` int(1) NOT NULL,
  `challengeok` int(1) NOT NULL,
  `handshakeok` int(1) NOT NULL,
  `address` varchar(100) DEFAULT NULL,
  `port` int(11) DEFAULT NULL,
  `hostport` int(11) DEFAULT NULL,
  `protocol` varchar(50) DEFAULT 'gamespy2',
  `type` varchar(50) DEFAULT 'swbf2',
  `gq_hostname` varchar(255) DEFAULT NULL,
  `gq_gametype` varchar(255) DEFAULT '0',
  `gq_mapname` varchar(255) DEFAULT NULL,
  `gq_maxplayers` int(11) DEFAULT '0',
  `gq_numplayers` int(11) DEFAULT '0',
  `gq_password` int(11) DEFAULT '0',
  `team0_name` varchar(50) DEFAULT NULL,
  `team0_score` int(11) DEFAULT '0',
  `team1_name` varchar(50) DEFAULT NULL,
  `team1_score` int(11) DEFAULT '0',
  `gamever` varchar(50) NOT NULL,
  `session` int(11) NOT NULL,
  `prevsession` int(11) NOT NULL,
  `servertype` int(1) NOT NULL,
  `gamemode` varchar(50) NOT NULL,
  `localips` varchar(255) NOT NULL,
  `clientid` int(11) NOT NULL,
  `netregion` int(1) NOT NULL,
  `custom` text NOT NULL,
  `lastseen` int(11) DEFAULT '0',
  `lastquery` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Tabellenstruktur für Tabelle `masterserver`
--

CREATE TABLE IF NOT EXISTS `masterserver` (
`id` int(11) NOT NULL,
  `server_name` varchar(32) DEFAULT NULL,
  `server_address` varchar(15) DEFAULT NULL,
  `server_port` int(11) DEFAULT NULL,
  `server_natnegaddress` varchar(100) NOT NULL,
  `server_natnegport` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Tabellenstruktur für Tabelle `natneg`
--

CREATE TABLE IF NOT EXISTS `natneg` (
`id` int(11) NOT NULL,
  `natneg_masterserver` int(11) NOT NULL,
  `natneg_cookie` int(11) NOT NULL DEFAULT '0',
  `natneg_gamename` varchar(50) DEFAULT NULL,
  `natneg_sequence` int(1) DEFAULT NULL,
  `natneg_localip` varchar(15) DEFAULT NULL,
  `natneg_localport` int(11) DEFAULT NULL,
  `natneg_remoteip` varchar(15) DEFAULT NULL,
  `natneg_remoteport` int(11) DEFAULT NULL,
  `natneg_clienttype` int(1) DEFAULT NULL,
  `natneg_comport` int(11) DEFAULT NULL,
  `natneg_lastupdate` int(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Tabellenstruktur für Tabelle `players`
--

CREATE TABLE IF NOT EXISTS `players` (
`id` int(10) NOT NULL,
  `sid` int(10) DEFAULT NULL,
  `gq_name` varchar(255) DEFAULT NULL,
  `gq_team` int(11) NOT NULL,
  `gq_kills` int(11) DEFAULT NULL,
  `gq_deaths` int(11) DEFAULT NULL,
  `gq_score` int(11) DEFAULT NULL,
  `gq_ping` int(11) unsigned DEFAULT NULL,
  `lastseen` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Tabellenstruktur für Tabelle `serverkeys`
--

CREATE TABLE IF NOT EXISTS `serverkeys` (
`id` int(11) NOT NULL,
  `key_gamename` varchar(20) DEFAULT NULL,
  `key_key` varchar(6) DEFAULT NULL,
  `supported` int(1) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Tabellenstruktur für Tabelle `stats_rounds`
--

CREATE TABLE IF NOT EXISTS `stats_rounds` (
`id` int(11) NOT NULL,
  `sid` int(11) NOT NULL,
  `uid` int(11) NOT NULL,
  `player` varchar(100) NOT NULL,
  `auth` varchar(255) NOT NULL,
  `pid` int(11) NOT NULL,
  `finishes` int(1) NOT NULL,
  `deaths` int(11) NOT NULL,
  `kills` int(11) NOT NULL,
  `endfaction` varchar(20) NOT NULL,
  `playerpoints` int(11) NOT NULL,
  `timePlayed` int(11) NOT NULL,
  `ctime` int(11) NOT NULL,
  `dtime` int(11) NOT NULL,
  `starts` int(11) NOT NULL,
  `heropoints` int(11) NOT NULL,
  `livingStreak` int(11) NOT NULL,
  `rating` int(11) NOT NULL,
  `mapname` varchar(50) NOT NULL,
  `winningTeam` varchar(20) NOT NULL,
  `gameComplete` int(1) NOT NULL,
  `winningCnt` int(11) NOT NULL,
  `losingCnt` int(11) NOT NULL,
  `gametype` varchar(50) NOT NULL,
  `losingTeam` varchar(50) NOT NULL,
  `GameMode` int(1) NOT NULL,
  `timestamp` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Tabellenstruktur für Tabelle `stats_servers`
--

CREATE TABLE IF NOT EXISTS `stats_servers` (
`id` int(11) NOT NULL,
  `title` varchar(255) NOT NULL,
  `desc` text NOT NULL,
  `ip` varchar(100) NOT NULL,
  `active` int(1) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Tabellenstruktur für Tabelle `users`
--

CREATE TABLE IF NOT EXISTS `users` (
`id` int(11) NOT NULL,
  `name` varchar(100) NOT NULL,
  `password` varchar(255) NOT NULL,
  `email` varchar(50) NOT NULL,
  `country` varchar(4) NOT NULL,
  `session` int(11) DEFAULT '0',
  `game` varchar(255) NOT NULL,
  `status` int(1) NOT NULL,
  `status_string` varchar(255) NOT NULL,
  `stats_finishes` int(11) NOT NULL,
  `stats_deaths` int(11) NOT NULL,
  `stats_kills` int(11) NOT NULL,
  `stats_playerpoints` int(11) NOT NULL,
  `stats_timePlayed` int(11) NOT NULL,
  `stats_ctime` int(11) NOT NULL,
  `stats_starts` int(11) NOT NULL,
  `stats_heropoints` int(11) NOT NULL,
  `stats_livingStreak` int(11) NOT NULL,
  `stats_rating` int(11) NOT NULL,
  `stats_gameComplete` int(11) NOT NULL,
  `stats_winningCnt` int(11) NOT NULL,
  `stats_losingCnt` int(11) NOT NULL,
  `stats_roundsplayed` int(11) NOT NULL,
  `stats_lastPlayed` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Indizes der exportierten Tabellen
--

--
-- Indizes für die Tabelle `gameserver`
--
ALTER TABLE `gameserver`
 ADD PRIMARY KEY (`id`);

--
-- Indizes für die Tabelle `masterserver`
--
ALTER TABLE `masterserver`
 ADD PRIMARY KEY (`id`);

--
-- Indizes für die Tabelle `natneg`
--
ALTER TABLE `natneg`
 ADD PRIMARY KEY (`id`);

--
-- Indizes für die Tabelle `players`
--
ALTER TABLE `players`
 ADD PRIMARY KEY (`id`), ADD KEY `gq_name` (`gq_name`), ADD KEY `lastseen` (`lastseen`), ADD KEY `sid` (`sid`);

--
-- Indizes für die Tabelle `serverkeys`
--
ALTER TABLE `serverkeys`
 ADD PRIMARY KEY (`id`);

--
-- Indizes für die Tabelle `stats_rounds`
--
ALTER TABLE `stats_rounds`
 ADD PRIMARY KEY (`id`);

--
-- Indizes für die Tabelle `stats_servers`
--
ALTER TABLE `stats_servers`
 ADD PRIMARY KEY (`id`);

--
-- Indizes für die Tabelle `users`
--
ALTER TABLE `users`
 ADD PRIMARY KEY (`id`);

--
-- AUTO_INCREMENT für exportierte Tabellen
--

--
-- AUTO_INCREMENT für Tabelle `gameserver`
--
ALTER TABLE `gameserver`
MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
--
-- AUTO_INCREMENT für Tabelle `masterserver`
--
ALTER TABLE `masterserver`
MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
--
-- AUTO_INCREMENT für Tabelle `natneg`
--
ALTER TABLE `natneg`
MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
--
-- AUTO_INCREMENT für Tabelle `players`
--
ALTER TABLE `players`
MODIFY `id` int(10) NOT NULL AUTO_INCREMENT;
--
-- AUTO_INCREMENT für Tabelle `serverkeys`
--
ALTER TABLE `serverkeys`
MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
--
-- AUTO_INCREMENT für Tabelle `stats_rounds`
--
ALTER TABLE `stats_rounds`
MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
--
-- AUTO_INCREMENT für Tabelle `stats_servers`
--
ALTER TABLE `stats_servers`
MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
--
-- AUTO_INCREMENT für Tabelle `users`
--
ALTER TABLE `users`
MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
