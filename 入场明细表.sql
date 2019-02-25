drop table if exists in_sessions;

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