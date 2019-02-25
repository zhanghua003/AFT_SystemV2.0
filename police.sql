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
   session_id           int comment '����ID',
   create_date          date comment '��������',
   buy_name             varchar(50) comment '��������',
   id_no                varchar(20) comment '����֤������',
   id_card_photo        mediumblob comment '���֤��Ƭ',
   year_ticket_photo    mediumblob comment '��Ʊ��Ƭ',
   address              varchar(200) comment '��ַ',
   status               int comment '״̬:0Ϊδ��,1Ϊ����,2Ϊͣ��',
   remark               varchar(500) comment '��ע',
   primary key (id)
);

alter table black_names comment '������';

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
   create_date          date comment '��������',
   session_id           int comment '����ID',
   key_content          varchar(100) comment '��Կ',
   key_type             varchar(100) comment '��Կ����',
   remark               varchar(500) comment '��ע',
   primary key (key_id)
);

alter table key_table comment '��Կ��';

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

alter table menus comment '�˵���';

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

alter table roles comment '��ɫ��';

/*==============================================================*/
/* Table: sessions                                              */
/*==============================================================*/
create table sessions
(
   session_id           int not null auto_increment comment '����ID',
   create_date          datetime comment '��������',
   session_name         varchar(100) comment '��������',
   session_date         datetime comment '����ʱ��',
   date_start           datetime comment '��ʼʱ��',
   date_end             datetime comment '����ʱ��',
   check_rule           int comment '0Ϊ��֤�Աȣ�1Ϊ��Ʊ��֤�Աȣ�2ΪɢƱ��֤�Ա�',
   status               int comment '0Ϊδ��ʼ��1Ϊ��ʼ��2Ϊ����',
   remark               varchar(500),
   primary key (session_id)
);

alter table sessions comment '���α�';

/*==============================================================*/
/* Table: users                                                 */
/*==============================================================*/
create table users
(
   user_id              int not null auto_increment,
   user_code            varchar(50) comment '�û���',
   user_name            varchar(50) comment '�û�����',
   password             varchar(36) comment '����',
   department_id        int comment '����ID',
   role_id              int comment '��ɫID',
   email                varchar(100) comment '����',
   mobile               varchar(50) comment '�ֻ�',
   tel_no               varchar(50) comment '�绰',
   address              varchar(200) comment '��ַ',
   create_date          date comment '��������',
   is_administrator     int comment '�Ƿ����Ա��1�ǣ�0��',
   status               int comment '�˺�״̬��0Ϊ����,1Ϊ����',
   primary key (user_id)
);

alter table users comment '�û���';

/*==============================================================*/
/* Table: white_names                                           */
/*==============================================================*/
create table white_names
(
   id                   int not null auto_increment,
   session_id           int comment '����ID',
   create_date          date comment '��������',
   buy_name             varchar(50) comment '��������',
   id_no                varchar(20) comment '����֤������',
   id_card_photo        mediumblob comment '���֤��Ƭ',
   year_ticket_photo    mediumblob comment '��Ʊ��Ƭ',
   address              varchar(200) comment '��ַ',
   ticket_type          int comment 'Ʊ������:0ΪɢƱ��1Ϊ��Ʊ',
   ticket_no            varchar(50) comment 'ɢƱ����',
   area                 varchar(20) comment '��',
   row                  varchar(20) comment '��',
   seat                 varchar(20) comment '��',
   tel_no               int comment '����',
   tel_area             varchar(50) comment '����',
   status               int comment '״̬:0Ϊδ��,1Ϊ����,2Ϊͣ��',
   remark               varchar(500) comment '��ע',
   primary key (id)
);

alter table white_names comment '������';


/*==============================================================*/
/* Table: in_sessions                                           */
/*==============================================================*/
create table in_sessions
(
   id                   int not null auto_increment,
   session_id           int comment '����ID',
   create_date          datetime comment '��������',
   name                 varchar(100) comment '��������',
   id_no                varchar(20) comment '����֤������',
   id_card_photo        mediumblob comment '���֤��Ƭ',
   take_photo           mediumblob comment '������Ƭ',
   id_address           varchar(200) comment '���֤��ַ',
   ticket_type          int comment 'Ʊ������:0ΪɢƱ��1Ϊ��Ʊ',
   ticket_no            varchar(50) comment 'ɢƱ����',
   area                 varchar(50) comment '��',
   row                  varchar(50) comment '��',
   seat                 varchar(50) comment '��',
   tel_no               varchar(50) comment '����',
   tel_area             varchar(50) comment '����',
   buy_name             varchar(50) comment '��Ʊ������',
   buy_photo            mediumblob comment '��Ʊ����Ƭ',
   buy_date             datetime comment '��������',
   validate_type        int comment '��֤��ʽ',
   sync_time            datetime comment 'ͬ��ʱ��',
   status               int comment '״̬',
   remark               varchar(500) comment '��ע',
   primary key (id)
);

alter table in_sessions comment '�볡��ϸ��';