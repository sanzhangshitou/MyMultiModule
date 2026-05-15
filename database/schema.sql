-- ============================================================
-- 商城商品管理系统数据库初始化脚本
-- 包含: 商品分类、商品主表、商品SKU、商品图片
-- ORM (SqlSugarCore) 自动将 CamelCase 属性映射为 snake_case 列名
-- ============================================================

CREATE DATABASE IF NOT EXISTS `my_mall` DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE `my_mall`;

-- ----------------------------
-- 商品分类表
-- ----------------------------
CREATE TABLE IF NOT EXISTS `product_category` (
  `id` BIGINT NOT NULL AUTO_INCREMENT COMMENT '主键',
  `name` VARCHAR(100) NOT NULL COMMENT '分类名称',
  `parent_id` BIGINT NOT NULL DEFAULT 0 COMMENT '父级分类ID, 0表示顶级分类',
  `sort_order` INT NOT NULL DEFAULT 0 COMMENT '排序值, 越小越靠前',
  `is_enabled` BIT NOT NULL DEFAULT 1 COMMENT '是否启用',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '更新时间',
  `is_deleted` BIT NOT NULL DEFAULT 0 COMMENT '软删除标记',
  PRIMARY KEY (`id`),
  INDEX `idx_parent_id` (`parent_id`),
  INDEX `idx_sort_order` (`sort_order`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='商品分类表';

-- ----------------------------
-- 商品主表
-- ----------------------------
CREATE TABLE IF NOT EXISTS `product` (
  `id` BIGINT NOT NULL AUTO_INCREMENT COMMENT '主键',
  `category_id` BIGINT NOT NULL COMMENT '所属分类ID',
  `name` VARCHAR(200) NOT NULL COMMENT '商品名称',
  `subtitle` VARCHAR(500) DEFAULT NULL COMMENT '商品副标题/简介',
  `description` TEXT COMMENT '商品详情(富文本)',
  `main_image` VARCHAR(500) DEFAULT NULL COMMENT '商品主图URL',
  `images` JSON DEFAULT NULL COMMENT '商品轮播图URL列表',
  `status` INT NOT NULL DEFAULT 0 COMMENT '商品状态: 0-草稿 1-上架 2-下架 3-禁用',
  `specs` JSON DEFAULT NULL COMMENT '规格定义, 如 [{"name":"颜色","values":["红","蓝"]}]',
  `unit` VARCHAR(20) DEFAULT NULL COMMENT '商品单位, 如 "件"/"箱"/"kg"',
  `sort_order` INT NOT NULL DEFAULT 0 COMMENT '排序值, 越小越靠前',
  `sales_count` INT NOT NULL DEFAULT 0 COMMENT '累计销量',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '更新时间',
  `is_deleted` BIT NOT NULL DEFAULT 0 COMMENT '软删除标记',
  PRIMARY KEY (`id`),
  INDEX `idx_category_id` (`category_id`),
  INDEX `idx_status` (`status`),
  INDEX `idx_sales_count` (`sales_count`),
  INDEX `idx_created_at` (`created_at`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='商品主表';

-- ----------------------------
-- 商品SKU表 (多规格库存)
-- ----------------------------
CREATE TABLE IF NOT EXISTS `product_sku` (
  `id` BIGINT NOT NULL AUTO_INCREMENT COMMENT '主键',
  `product_id` BIGINT NOT NULL COMMENT '所属商品ID',
  `sku_code` VARCHAR(100) DEFAULT NULL COMMENT 'SKU编码',
  `spec_values` JSON DEFAULT NULL COMMENT '规格值组合, 如 {"颜色":"红色","尺寸":"XL"}',
  `price` DECIMAL(10,2) NOT NULL COMMENT '销售价格',
  `market_price` DECIMAL(10,2) DEFAULT NULL COMMENT '市场价格(划线价)',
  `stock` INT NOT NULL DEFAULT 0 COMMENT '库存数量',
  `image` VARCHAR(500) DEFAULT NULL COMMENT 'SKU图片(覆盖商品主图)',
  `is_enabled` BIT NOT NULL DEFAULT 1 COMMENT '是否启用该规格',
  `sort_order` INT NOT NULL DEFAULT 0 COMMENT '排序值',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '更新时间',
  `is_deleted` BIT NOT NULL DEFAULT 0 COMMENT '软删除标记',
  PRIMARY KEY (`id`),
  INDEX `idx_product_id` (`product_id`),
  INDEX `idx_sku_code` (`sku_code`),
  INDEX `idx_price` (`price`),
  INDEX `idx_stock` (`stock`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='商品SKU表';

-- ----------------------------
-- 商品图片表 (支持按SKU关联)
-- ----------------------------
CREATE TABLE IF NOT EXISTS `product_image` (
  `id` BIGINT NOT NULL AUTO_INCREMENT COMMENT '主键',
  `product_id` BIGINT NOT NULL COMMENT '所属商品ID',
  `sku_id` BIGINT DEFAULT NULL COMMENT '关联的SKU ID (可为空，为空表示商品级图片)',
  `url` VARCHAR(500) NOT NULL COMMENT '图片URL',
  `sort_order` INT NOT NULL DEFAULT 0 COMMENT '排序值',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  PRIMARY KEY (`id`),
  INDEX `idx_product_id` (`product_id`),
  INDEX `idx_sku_id` (`sku_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='商品图片表';

-- ----------------------------
-- 初始化分类数据
-- ----------------------------
INSERT INTO `product_category` (`id`, `name`, `parent_id`, `sort_order`) VALUES
(1, '服装', 0, 1),
(2, '电子产品', 0, 2),
(3, '食品饮料', 0, 3),
(4, '男装', 1, 1),
(5, '女装', 1, 2),
(6, '手机', 2, 1),
(7, '电脑', 2, 2);
