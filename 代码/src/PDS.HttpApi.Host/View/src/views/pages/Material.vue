<template>
  <a-layout class="hb-layout">
    <a-layout-sider class="default-layout-sider">
      <menu-sider name="物料管理" :menus="menus"></menu-sider>
    </a-layout-sider>
    <a-layout-content class="hb-layout-content">
      <router-view></router-view>
    </a-layout-content>
  </a-layout>
</template>
<script>
import "@/components/style/components.less";
import menus from "@/menu/modules/material";
export default {
  name: "Material",
  data() {
    return {
      menus: menus,
      // menus: [
      //   {
      //     name: "stock",
      //     displayName: "库存管理",
      //     icon: "bars",
      //     route: "/material/stock",
      //     isGranted: true,
      //     items: [
      //       {
      //         name: "in",
      //         displayName: "入库明细",
      //         icon: "profile",
      //         route: "/material/stock/in",
      //         isGranted: true,
      //       },
      //       {
      //         name: "out",
      //         displayName: "出库明细",
      //         icon: "profile",
      //         route: "/material/stock/out",
      //         isGranted: true,
      //       },
      //       {
      //         name: "statis",
      //         displayName: "库存统计",
      //         icon: "database",
      //         route: "/material/stock/statistic",
      //         isGranted: true,
      //       },
      //     ],
      //   },
      //   {
      //     name: "list",
      //     displayName: "物料维护",
      //     icon: "container",
      //     route: "/material/list",
      //     isGranted: true,
      //   },
      // ],
    };
  },
};
</script>
