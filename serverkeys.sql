/*
Navicat MySQL Data Transfer

Source Server         : localhost
Source Server Version : 50527
Source Host           : localhost:3306
Source Database       : swbf2

Target Server Type    : MYSQL
Target Server Version : 50527
File Encoding         : 65001

Date: 2014-05-28 19:13:50
*/

SET FOREIGN_KEY_CHECKS=0;

-- ----------------------------
-- Table structure for `serverkeys`
-- ----------------------------
DROP TABLE IF EXISTS `serverkeys`;
CREATE TABLE `serverkeys` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `key_gamename` varchar(20) DEFAULT NULL,
  `key_key` varchar(6) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=latin1;

-- ----------------------------
-- Records of serverkeys
-- ----------------------------
INSERT INTO `serverkeys` VALUES ('0', 'swbfrontpc', 'y3Hd2d');
INSERT INTO `serverkeys` VALUES ('1', 'swbfront2pc', 'hMO2d4');
