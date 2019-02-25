/*==============================================================*/
/* DBMS name:      MySQL 5.0                                    */
/* Created on:     2018/1/13 19:20:58                           */
/*==============================================================*/


drop table if exists black_names;

drop table if exists departments;

drop table if exists key_table;

drop table if exists menus;

drop table if exists role_menu;

drop table if exists roles;

drop table if exists sessions;

drop table if exists users;

drop table if exists white_names;

drop table if exists in_sessions;
/*==============================================================*/
/* Table: black_names                                           */
/*==============================================================*/
create table black_names
(
   id                   int not null auto_increment,
   session_id           int comment '场次ID',
   create_date          date comment '创建日期',
   buy_name             varchar(50) comment '购买姓名',
   id_no                varchar(20) comment '购买证件号码',
   id_card_photo        mediumblob comment '身价证照片',
   year_ticket_photo    mediumblob comment '年票照片',
   address              varchar(200) comment '地址',
   status               int comment '状态:0为未用,1为启用,2为停用',
   remark               varchar(500) comment '备注',
   primary key (id)
);

alter table black_names comment '黑名单';

/*==============================================================*/
/* Table: departments                                           */
/*==============================================================*/
create table departments
(
   department_id        int not null auto_increment,
   department_name      varchar(50),
   status               int,
   primary key (department_id)
);

/*==============================================================*/
/* Table: key_table                                             */
/*==============================================================*/
create table key_table
(
   key_id               int not null auto_increment,
   create_date          date comment '创建日期',
   session_id           int comment '场次ID',
   key_content          varchar(100) comment '密钥',
   key_type             varchar(100) comment '密钥类型',
   remark               varchar(500) comment '备注',
   primary key (key_id)
);

alter table key_table comment '密钥表';

/*==============================================================*/
/* Table: menus                                                 */
/*==============================================================*/
create table menus
(
   menu_id              int not null auto_increment,
   pid                  int,
   name                 varchar(50),
   url                  varchar(500) default '#',
   icon_class           varchar(50),
   status               int default 1,
   sort                 int default 99,
   primary key (menu_id)
);

alter table menus comment '菜单表';

/*==============================================================*/
/* Table: role_menu                                             */
/*==============================================================*/
create table role_menu
(
   id                   int not null auto_increment,
   role_id              int,
   menu_id              int,
   primary key (id)
);

/*==============================================================*/
/* Table: roles                                                 */
/*==============================================================*/
create table roles
(
   role_id              int not null auto_increment,
   role_name            varchar(50),
   create_date          date,
   remark               varchar(500),
   primary key (role_id)
);

alter table roles comment '角色表';

/*==============================================================*/
/* Table: sessions                                              */
/*==============================================================*/
create table sessions
(
   session_id           int not null auto_increment comment '场次ID',
   create_date          datetime comment '创建日期',
   session_name         varchar(100) comment '场次名称',
   session_date         datetime comment '场次时间',
   date_start           datetime comment '开始时间',
   date_end             datetime comment '结束时间',
   check_rule           int comment '0为人证对比，1为套票人证对比，2为散票人证对比',
   status               int comment '0为未开始，1为开始，2为结束',
   remark               varchar(500),
   primary key (session_id)
);

alter table sessions comment '场次表';

/*==============================================================*/
/* Table: users                                                 */
/*==============================================================*/
create table users
(
   user_id              int not null auto_increment,
   user_code            varchar(50) comment '用户号',
   user_name            varchar(50) comment '用户姓名',
   password             varchar(36) comment '密码',
   department_id        int comment '部门ID',
   role_id              int comment '角色ID',
   email                varchar(100) comment '邮箱',
   mobile               varchar(50) comment '手机',
   tel_no               varchar(50) comment '电话',
   address              varchar(200) comment '地址',
   create_date          date comment '创建日期',
   is_administrator     int comment '是否管理员，1是，0否',
   status               int comment '账号状态：0为禁用,1为启用',
   primary key (user_id)
);

alter table users comment '用户表';

/*==============================================================*/
/* Table: white_names                                           */
/*==============================================================*/
create table white_names
(
   id                   int not null auto_increment,
   session_id           int comment '场次ID',
   create_date          date comment '创建日期',
   buy_name             varchar(50) comment '购买姓名',
   id_no                varchar(20) comment '购买证件号码',
   id_card_photo        mediumblob comment '身价证照片',
   year_ticket_photo    mediumblob comment '年票照片',
   address              varchar(200) comment '地址',
   ticket_type          int comment '票务类型:0为散票，1为年票',
   ticket_no            varchar(50) comment '散票号码',
   area                 varchar(20) comment '区',
   row                  varchar(20) comment '排',
   seat                 varchar(20) comment '座',
   tel_no               int comment '机号',
   tel_area             varchar(50) comment '区域',
   status               int comment '状态:0为未用,1为启用,2为停用',
   remark               varchar(500) comment '备注',
   primary key (id)
);

alter table white_names comment '白名单';


/*==============================================================*/
/* Table: in_sessions                                           */
/*==============================================================*/
create table in_sessions
(
   id                   int not null auto_increment,
   session_id           int comment '场次ID',
   create_date          datetime comment '创建日期',
   name                 varchar(100) comment '场次名称',
   id_no                varchar(20) comment '购买证件号码',
   id_card_photo        mediumblob comment '身价证照片',
   take_photo           mediumblob comment '拍照照片',
   id_address           varchar(200) comment '身份证地址',
   ticket_type          int comment '票务类型:0为散票，1为年票',
   ticket_no            varchar(50) comment '散票号码',
   area                 varchar(50) comment '区',
   row                  varchar(50) comment '排',
   seat                 varchar(50) comment '座',
   tel_no               varchar(50) comment '机号',
   tel_area             varchar(50) comment '区域',
   buy_name             varchar(50) comment '购票人姓名',
   buy_photo            mediumblob comment '购票人照片',
   buy_date             datetime comment '购买日期',
   validate_type        int comment '验证方式',
   sync_time            datetime comment '同步时间',
   status               int comment '状态',
   remark               varchar(500) comment '备注',
   primary key (id)
);

alter table in_sessions comment '入场明细表';