<template>
  <a-layout class="hb-layout">
    <a-layout-header class="content-layout-header">
      <a-button
        type="primary"
        icon="plus"
        size="small"
        @click="show"
        v-if="isGranted('AbpIdentity.Users.Create')"
      >
        {{ $t("actions.add") }}
      </a-button>
    </a-layout-header>
    <a-layout-content class="content-layout-content">
      <a-table
        :columns="columns"
        :data-source="data"
        bordered
        :rowKey="(row, index) => row.id"
        size="small"
        :pagination="pagination"
        :loading="loading"
        @change="change"
      >
        <template slot="isActive" slot-scope="text, record">
          {{ record.isActive ? $t("title.enable") : $t("title.forbidden") }}
        </template>
        <template slot="operation" slot-scope="text, record">
          <a
            href="javascript:void(0);"
            @click="show('edit', record)"
            v-if="isGranted('AbpIdentity.Users.Update')"
          >
            <a-icon type="edit" />{{ $t("actions.edit") }}
          </a>
          <a-popconfirm
            :title="$t('confirm.delete')"
            @confirm="remove(record)"
            v-if="isGranted('AbpIdentity.Users.Delete')"
          >
            <a href="javascript:;">
              <a-icon type="delete" />{{ $t("actions.delete") }}
            </a>
          </a-popconfirm>
        </template>
      </a-table>
    </a-layout-content>
    <user-form v-if="showForm" ref="form" @close="close" />
  </a-layout>
</template>
<script>
import locale from "@/localize/system/user";
import { Pagination, Delete } from "@/api/user";
import UserForm from "./UserForm.vue";
import component from "@/lib/base";
export default {
  name: "UserManager",
  mixins: [component],
  i18n: {
    messages: locale,
  },
  components: { UserForm },
  data() {
    return {
      columns: [
        {
          dataIndex: "userName",
          title: this.$t("title.name"),
          align: "center",
        },
        {
          dataIndex: "email",
          title: this.$t("title.email"),
          align: "center",
        },
        {
          dataIndex: "phoneNumber",
          title: this.$t("title.phoneNumber"),
          align: "center",
        },
        {
          dataIndex: "isActive",
          title: this.$t("title.isActive"),
          align: "center",
          scopedSlots: { customRender: "isActive" },
        },
        {
          dataIndex: "Actions",
          title: this.$t("title.actions"),
          align: "center",
          scopedSlots: { customRender: "operation" },
        },
      ],
      data: [],
      showForm: false,
      pagination: {
        current: 1,
        total: 0,
        pageSize: 10,
        "show-size-changer": true,
        "show-total": (total) => `共${total}条数据`,
      },
      loading: false,
    };
  },
  mounted() {
    this.load();
  },
  methods: {
    load: async function () {
      let _this = this;
      try {
        this.loading = true;
        let response = await Pagination({
          skipCount: (this.pagination.current - 1) * this.pagination.pageSize,
          maxResultCount: this.pagination.pageSize,
        });
        _this.data = response.items;
        _this.total = response.totalCount;
      } catch (error) {
        this.$message.error(_this.$t("load.userError"));
        console.log(error);
      } finally {
        this.loading = false;
      }
    },
    show: function (action, record) {
      let _this = this;
      _this.showForm = true;
      _this.$nextTick(() => {
        if (record) _this.$refs.form.load(record ? record.id : 0);
      });
      //this.$refs.form.show(record ? record.id : 0);
    },
    close: function () {
      this.showForm = false;
      this.load();
    },
    remove: async function (record) {
      let _this = this;
      let spin = this.$spin({ text: _this.$t("submit.delete") });
      try {
        await Delete(record.id);
        _this.$message.success(_this.$t("save.delete"));
        _this.load();
      } catch (err) {
        _this.$message.error(err.message);
      } finally {
        spin.close();
      }
    },
    change: function (pagination) {
      this.pagination = pagination;
      this.load();
    },
  },
};
</script>
