<template>
  <a-layout class="hb-layout">
    <a-layout-sider class="default-layout-sider">
      <menu-sider :name="$t('title.system')" :menus="menus"></menu-sider>
    </a-layout-sider>
    <a-layout-content class="hb-layout-content">
      <router-view></router-view>
    </a-layout-content>
  </a-layout>
</template>
<script>
import component from "@/lib/base";
import locale from "@/localize/locale";
import "@/components/style/components.less";
import menus from "@/menu/modules/system";
export default {
  name: "System",
  i18n: {
    messages: locale,
  },
  mixins: [component],
  data() {
    return {
      menus: menus,
      // menus: [
      //   {
      //     name: "users",
      //     displayName: this.$t("menus.userManager"),
      //     icon: "team",
      //     isGranted: true,
      //     items: [
      //       {
      //         name: "manager",
      //         displayName: this.$t("menus.userList"),
      //         icon: "user",
      //         isGranted: this.isGranted("AbpIdentity.Users"),
      //         route: "/system/users/manager",
      //       },
      //       {
      //         name: "roles",
      //         displayName: this.$t("menus.roles"),
      //         icon: "idcard",
      //         isGranted: this.isGranted("AbpIdentity.Roles"),
      //         route: "/system/users/roles",
      //       },
      //     ],
      //   },
      //   {
      //     name: "apis",
      //     displayName: this.$t("menus.audit"),
      //     icon: "api",
      //     isGranted: this.isGranted("Aduit.List"),
      //     route: "/system/audit",
      //     items: [],
      //   },
      //   {
      //     name: "settings",
      //     displayName: this.$t("menus.global"),
      //     icon: "edit",
      //     isGranted: this.isGranted("Setting.Global"),
      //     route: "/system/settings",
      //   },
      // ],
    };
  },
};
</script>
